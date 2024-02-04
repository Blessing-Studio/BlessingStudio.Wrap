using BlessingStudio.Wrap;
using BlessingStudio.Wrap.Client;
using BlessingStudio.Wrap.Client.Commands;
using BlessingStudio.Wrap.Client.Managers;
using BlessingStudio.Wrap.Interfaces;
using BlessingStudio.Wrap.Utils;
using ConsoleInteractive;
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
void Client_OnDeviceFound(object Sender, DeviceLocationEventArgs e)
{
    if(e.Location.GetDevice().Device.DeviceType != "urn:schemas-upnp-org:device:InternetGatewayDevice:1") return;
    if(e.RemoteEndPoint.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) return;
    if (Searching)
        ConsoleWriter.WriteLine(e.Location.GetDevice().Device.FriendlyName);
    UPnPDeviceLocations.Add(e.Location.GetDevice().Device);
}

client.StartSearch();
ConsoleWriter.WriteLine("开始查找UPnP设备 超时时间" + timeout.TotalSeconds);
DateTime time = DateTime.Now;
while((DateTime.Now - time) < timeout && UPnPDeviceLocations.Count == 0)
{
    Thread.Sleep(1);
}
Searching = false;
foreach (UPnPDevice UPnPDeviceLocation in UPnPDeviceLocations)
{
    if (UPnPDeviceLocation != null)
    {
        UPnPService? natService = UPnPDeviceLocation.GetService("urn:schemas-upnp-org:service:WANIPConnection:1"); // 获取WAN IP连接服务
        natService ??= UPnPDeviceLocation.GetService("urn:schemas-upnp-org:service:WANPPPConnection:1");
        if (natService != null)
        {
            uPnP = new UPnP(natService);
            ConsoleWriter.WriteLine("UPnP已支持");
            break;
        }
    }
}
ConsoleWriter.WriteLine("NAT类型");
ConsoleWriter.WriteLine(StunUtils.GetNatType().ToString());
if (uPnP != null)
{
    ConsoleWriter.WriteLine("UPnP NAT类型");
    IPEndPoint endPoint = new(new IPAddress(new byte[] { 0, 0, 0, 0 }), RandomUtils.GetRandomPort());
    uPnP.AddPortMapping(endPoint.Port, SocketProtocol.UDP, endPoint.Port, "Wrap NAT test");
    ClassicStunResult result = StunUtils.GetClassicStunResultAsync(endPoint).GetAwaiter().GetResult();
    ConsoleWriter.WriteLine(result.NatType.ToString());
    uPnP.DeletePortMapping(endPoint.Port, SocketProtocol.UDP);
}
IWrapClient wrapClient = new WrapClient();
Thread.Sleep(1000);
wrapClient.Connect(new IPAddress(new byte[] { 8, 137, 84, 98 }));
MakeRequestCommand requestCommand = new(wrapClient);
AcceptRequestCommand acceptRequestCommand = new(wrapClient);
SetServerCommand setServerCommand = new(wrapClient);
commandManager.RegisterCommandExecuter(requestCommand);
commandManager.RegisterCommandTabCompleter(requestCommand);
commandManager.RegisterCommandExecuter(acceptRequestCommand);
commandManager.RegisterCommandTabCompleter(acceptRequestCommand);
commandManager.RegisterCommandExecuter(setServerCommand);
commandManager.RegisterCommandTabCompleter(setServerCommand);

wrapClient.ExpectedDisconnect += e =>
{
    ConsoleWriter.WriteLine($"与{e.UserToken}断开连接\n原因 {e.Reason}");
};

wrapClient.UnexpectedDisconnect += e =>
{
    ConsoleWriter.WriteLine($"与{e.UserToken}断开连接");
};

wrapClient.ConnectPeerSuccessfully += e =>
{
    ConsoleWriter.WriteLine($"与{e.UserToken}连接成功 已映射到localhost:{e.Port}");
};

wrapClient.RequestInvalidated += e =>
{
    ConsoleWriter.WriteLine($"给{e.Requester}发出的请求已失效");
};

wrapClient.ReconnectPeer += e =>
{
    ConsoleWriter.WriteLine($"开始与{e.UserToken}反向打洞");
};

wrapClient.LoginedSuccessfully += e =>
{
    ConsoleWriter.WriteLine("登录成功");
    ConsoleWriter.WriteLine($"UserToken   {e.UserToken}");

    ConsoleReader.BeginReadThread();
};

wrapClient.NewRequest += e =>
{
    ConsoleWriter.WriteLine($"收到{e.Requester}的请求");
};

ConsoleReader.MessageReceived += (sender, s) => {
    commandManager.Execute(s.TrimStart());
};

ConsoleReader.OnInputChange += (sender, e) =>
{
    if(e.Text == string.Empty)
    {
        ConsoleSuggestion.ClearSuggestions();
        return;
    }
    IList<string>? strings = null;
    if (!e.Text.TrimStart().Contains(' '))
    {
        strings = commandManager.Executors.Select(c => c.GetName()).ToList();
    }
    strings ??= commandManager.Complete(e.Text.TrimStart());
    ConsoleSuggestion.ClearSuggestions();
    List<ConsoleSuggestion.Suggestion> suggestions = new();
    foreach (string s in strings)
    {
        ConsoleSuggestion.Suggestion suggestion = new(s);
        suggestions.Add(suggestion);
    }
    int offset = 0;
    int offset2 = e.Text.Length;
    if(e.Text.LastIndexOf(" ") != -1)
    {
        offset = e.Text.LastIndexOf(" ") + 1;
    }
    ConsoleSuggestion.UpdateSuggestions(suggestions.ToArray(), new(offset, offset2));
};


ConsoleSuggestion.SetMaxSuggestionCount(8);

wrapClient.Start();
void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
{
    ConsoleWriter.WriteLine(e.ExceptionObject.ToString()!);
}
