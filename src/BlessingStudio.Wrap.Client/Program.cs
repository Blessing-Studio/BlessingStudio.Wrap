using BlessingStudio.Wrap;
using BlessingStudio.Wrap.Client;
using BlessingStudio.Wrap.Interfaces;
using BlessingStudio.Wrap.Utils;
using STUN.StunResult;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Waher.Networking.UPnP;
using static BlessingStudio.Wrap.Interfaces.IUPnPService;

TimeSpan timeout = TimeSpan.FromSeconds(10);
AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
List<UPnPDevice> UPnPDeviceLocations = new();
IUPnPService? uPnP = null;
bool Searching = true;
UPnPClient client = new();
client.OnDeviceFound += Client_OnDeviceFound;
void Client_OnDeviceFound(object Sender, DeviceLocationEventArgs e)
{
    if(e.Location.GetDevice().Device.DeviceType != "urn:schemas-upnp-org:device:InternetGatewayDevice:1") return;
    if(e.RemoteEndPoint.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) return;
    if (Searching)
        Console.WriteLine(e.Location.GetDevice().Device.FriendlyName);
    UPnPDeviceLocations.Add(e.Location.GetDevice().Device);
}

client.StartSearch();
Console.WriteLine("开始查找UPnP设备");
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
        if(natService is null)
        {
            natService = UPnPDeviceLocation.GetService("urn:schemas-upnp-org:service:WANPPPConnection:1");
        }
        if (natService != null)
        {
            uPnP = new UPnP(natService);
            Console.WriteLine("UPnP已支持");
            break;
        }
    }
}
Console.WriteLine("NAT类型");
Console.WriteLine(StunUtils.GetNatType());
if (uPnP != null)
{
    Console.WriteLine("UPnP NAT类型");
    IPEndPoint endPoint = new(new IPAddress(new byte[] { 0, 0, 0, 0 }), RandomUtils.GetRandomPort());
    uPnP.AddPortMapping(endPoint.Port, SocketProtocol.UDP, endPoint.Port, "Wrap NAT test");
    ClassicStunResult result = StunUtils.GetClassicStunResultAsync(endPoint).GetAwaiter().GetResult();
    Console.WriteLine(result.NatType.ToString());
    uPnP.DeletePortMapping(endPoint.Port, SocketProtocol.UDP);
}
WrapClient wrapClient = new();
Thread.Sleep(1000);
wrapClient.Connect(new IPAddress(new byte[] { 8, 137, 84, 98 }));

wrapClient.ExpectedDisconnect += e =>
{
    Console.WriteLine($"与{e.UserToken}断开连接\n原因 {e.Reason}");
};

wrapClient.UnexpectedDisconnect += e =>
{
    Console.WriteLine($"与{e.UserToken}断开连接");
};

wrapClient.ConnectPeerSuccessfully += e =>
{
    Console.WriteLine($"与{e.UserToken}连接成功");
};

wrapClient.ReconnectPeer += e =>
{
    Console.WriteLine($"开始与{e.UserToken}反向打洞");
};

wrapClient.LoginedSuccessfully += e =>
{
    Console.WriteLine("登录成功");
    Console.WriteLine($"UserToken   {e.UserToken}");
    Task.Run(() =>
    {
        string token = Console.ReadLine()!;
        wrapClient.MakeRequest(token);
    });
};

wrapClient.NewRequest += e =>
{
    wrapClient.AcceptRequest(e.RequestInfo);
};

wrapClient.Start();
void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
{
    Console.WriteLine(e.ExceptionObject.ToString());
}
