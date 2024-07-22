using BlessingStudio.Wrap;
using BlessingStudio.Wrap.Client;
using BlessingStudio.Wrap.Client.Commands;
using BlessingStudio.Wrap.Client.Managers;
using BlessingStudio.Wrap.Interfaces;
using BlessingStudio.Wrap.Utils;
using STUN.StunResult;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Waher.Networking.UPnP;
using Waher.Script.Functions.Analytic;
using static BlessingStudio.Wrap.Interfaces.IUPnPService;

TimeSpan timeout = TimeSpan.FromSeconds(30);
AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
List<UPnPDevice> UPnPDeviceLocations = new();
IUPnPService? uPnP = null;
bool Searching = true;
UPnPClient client = new();
client.OnDeviceFound += Client_OnDeviceFound;
CommandManager commandManager = new();
void Client_OnDeviceFound(object Sender, DeviceLocationEventArgs e) {
    if (e.Location.GetDevice().Device.DeviceType != "urn:schemas-upnp-org:device:InternetGatewayDevice:1") return;
    if (e.RemoteEndPoint.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) return;
    if (Searching)
        Console.WriteLine(e.Location.GetDevice().Device.FriendlyName);
    UPnPDeviceLocations.Add(e.Location.GetDevice().Device);
}

client.StartSearch();
Console.WriteLine("开始查找UPnP设备 超时时间" + timeout.TotalSeconds);
DateTime time = DateTime.Now;
while ((DateTime.Now - time) < timeout && UPnPDeviceLocations.Count == 0) {
    Thread.Sleep(1);
}
Searching = false;
foreach (UPnPDevice UPnPDeviceLocation in UPnPDeviceLocations) {
    if (UPnPDeviceLocation != null) {
        UPnPService? natService = UPnPDeviceLocation.GetService("urn:schemas-upnp-org:service:WANIPConnection:1"); // 获取WAN IP连接服务
        natService ??= UPnPDeviceLocation.GetService("urn:schemas-upnp-org:service:WANPPPConnection:1");
        if (natService != null) {
            uPnP = new UPnP(natService);
            Console.WriteLine("UPnP已支持");
            break;
        }
    }
}
Console.WriteLine("NAT类型");
Console.WriteLine(StunUtils.GetNatType().ToString());
if (uPnP != null) {
    Console.WriteLine("UPnP NAT类型");
    IPEndPoint endPoint = new(new IPAddress(new byte[] { 0, 0, 0, 0 }), RandomUtils.GetRandomPort());
    uPnP.AddPortMapping(endPoint.Port, SocketProtocol.UDP, endPoint.Port, "Wrap NAT test");
    ClassicStunResult result = StunUtils.GetClassicStunResultAsync(endPoint).GetAwaiter().GetResult();
    Console.WriteLine(result.NatType.ToString());
    uPnP.DeletePortMapping(endPoint.Port, SocketProtocol.UDP);
}
IWrapClient wrapClient = new WrapClient();
Thread.Sleep(1000);
wrapClient.Connect(Dns.GetHostAddresses("wrap.blessing-studio.tech").First());
MakeRequestCommand requestCommand = new(wrapClient);
AcceptRequestCommand acceptRequestCommand = new(wrapClient);
SetServerCommand setServerCommand = new(wrapClient);
KickCommand kickCommand = new(wrapClient);
AutoAcceptCommand autoAcceptCommand = new(wrapClient);
commandManager.RegisterCommandExecuter(requestCommand);
commandManager.RegisterCommandTabCompleter(requestCommand);
commandManager.RegisterCommandExecuter(acceptRequestCommand);
commandManager.RegisterCommandTabCompleter(acceptRequestCommand);
commandManager.RegisterCommandExecuter(setServerCommand);
commandManager.RegisterCommandTabCompleter(setServerCommand);
commandManager.RegisterCommandExecuter(kickCommand);
commandManager.RegisterCommandTabCompleter(kickCommand);
commandManager.RegisterCommandExecuter(autoAcceptCommand);
commandManager.RegisterCommandTabCompleter(autoAcceptCommand);

wrapClient.ExpectedDisconnect += e => {
    Console.WriteLine($"与{e.UserToken}断开连接\n原因 {e.Reason}");
};

wrapClient.UnexpectedDisconnect += e => {
    Console.WriteLine($"与{e.UserToken}断开连接");
};

wrapClient.ConnectPeerSuccessfully += e => {
    Console.WriteLine($"与{e.UserToken}连接成功 已映射到localhost:{e.Port}");
};

wrapClient.RequestInvalidated += e => {
    Console.WriteLine($"给{e.Requester}发出的请求已失效");
};

wrapClient.ReconnectPeer += e => {
    Console.WriteLine($"开始与{e.UserToken}反向打洞");
};

wrapClient.ConnectFailed += e => {
    Console.WriteLine($"与{e.UserToken}连接失败");
};

wrapClient.NewRequest += e => {
    Console.WriteLine($"收到{e.Requester}的请求");
};

wrapClient.LoginedSuccessfully += e => {
    Console.WriteLine("登录成功");
    Console.WriteLine($"UserToken   {e.UserToken}");

    try {
        while (true) {
            var text = Console.ReadLine();
            commandManager.Execute(text!.TrimStart());
        }
    } catch { }
};



//ConsoleReader.MessageReceived += (sender, s) => {
//    commandManager.Execute(s.TrimStart());
//};

//ConsoleReader.OnInputChange += (sender, e) => {
//    if (e.Text == string.Empty) {
//        ConsoleSuggestion.ClearSuggestions();
//        return;
//    }
//    IList<string>? strings = null;
//    if (!e.Text.TrimStart().Contains(' ')) {
//        strings = commandManager.Executors.Select(c => c.GetName()).ToList();
//    }
//    strings ??= commandManager.Complete(e.Text.TrimStart());
//    ConsoleSuggestion.ClearSuggestions();
//    List<ConsoleSuggestion.Suggestion> suggestions = new();
//    foreach (string s in strings) {
//        ConsoleSuggestion.Suggestion suggestion = new(s);
//        suggestions.Add(suggestion);
//    }
//    int offset = 0;
//    int offset2 = e.Text.Length;
//    if (e.Text.LastIndexOf(" ") != -1) {
//        offset = e.Text.LastIndexOf(" ") + 1;
//    }
//    ConsoleSuggestion.UpdateSuggestions(suggestions.ToArray(), new(offset, offset2));
//};

wrapClient.Start();

void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
    Console.WriteLine(e.ExceptionObject.ToString()!);
}