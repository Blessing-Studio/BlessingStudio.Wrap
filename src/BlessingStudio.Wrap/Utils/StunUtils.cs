using STUN.Client;
using STUN.Enums;
using STUN.StunResult;
using System.Net;
using System.Net.Sockets;

namespace BlessingStudio.Wrap.Utils;

public static class StunUtils
{
    public static string STUNServer { get; set; } = "stun.miwifi.com";
    public static IPEndPoint GetRemoteIP(IPEndPoint localIp)
    {
        return GetRemoteIPAsync(localIp).GetAwaiter().GetResult();
    }
    public static IPEndPoint GetRemoteIP(Socket socket)
    {
        return GetRemoteIPAsync(socket).GetAwaiter().GetResult();
    }
    public static async Task<IPEndPoint> GetRemoteIPAsync(IPEndPoint localIp)
    {
        using StunClient3489 stunClient = new(new((await Dns.GetHostAddressesAsync(STUNServer)).First(), 3478), localIp);
        await stunClient.QueryAsync();
        return stunClient.State.PublicEndPoint!;
    }

    public static async Task<IPEndPoint> GetRemoteIPAsync(Socket socket)
    {
        using StunClient3489 stunClient = new(new((await Dns.GetHostAddressesAsync(STUNServer)).First(), 3478), (IPEndPoint)socket.LocalEndPoint!, new NoneUdpProxy(socket));
        await stunClient.QueryAsync();
        return stunClient.State.PublicEndPoint!;
    }
    public static NatType GetNatType()
    {
        return GetNatTypeAsync().GetAwaiter().GetResult();
    }
    public static async Task<NatType> GetNatTypeAsync()
    {
        using StunClient3489 stunClient = new(new((await Dns.GetHostAddressesAsync(STUNServer)).First(), 3478), IPEndPoint.Parse("0.0.0.0"));
        await stunClient.QueryAsync();
        return stunClient.State.NatType;
    }

    public static async Task<ClassicStunResult> GetClassicStunResultAsync(IPEndPoint localIp)
    {
        using StunClient3489 stunClient = new(new((await Dns.GetHostAddressesAsync(STUNServer)).First(), 3478), IPEndPoint.Parse("0.0.0.0"), new NoneUdpProxy(localIp));
        await stunClient.QueryAsync();
        return stunClient.State;
    }

    public static async Task<ClassicStunResult> GetClassicStunResultAsync()
    {
        using StunClient3489 stunClient = new(new((await Dns.GetHostAddressesAsync(STUNServer)).First(), 3478), IPEndPoint.Parse("0.0.0.0"));
        await stunClient.QueryAsync();
        return stunClient.State;
    }
}
