using STUN.Client;
using STUN.Enums;
using STUN.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BlessingStudio.Wrap.Utils
{
    public static class StunUtils
    {
        public static string STUNServer = "stun.qq.com";
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
            using StunClient3489 stunClient = new(new(Dns.GetHostAddresses(STUNServer).First(), 3478), localIp);
            await stunClient.QueryAsync();
            return stunClient.State.PublicEndPoint;
        }

        public static async Task<IPEndPoint> GetRemoteIPAsync(Socket socket)
        {
            using StunClient3489 stunClient = new(new(Dns.GetHostAddresses(STUNServer).First(), 3478), (IPEndPoint)socket.LocalEndPoint, new NoneUdpProxy(socket));
            await stunClient.QueryAsync();
            return stunClient.State.PublicEndPoint;
        }
        public static NatType GetNatType()
        {
            return GetNatTypeAsync().GetAwaiter().GetResult();
        }
        public static async Task<NatType> GetNatTypeAsync()
        {
            using StunClient3489 stunClient = new(new(Dns.GetHostAddresses(STUNServer).First(), 3478), IPEndPoint.Parse("0.0.0.0"));
            await stunClient.QueryAsync();
            return stunClient.State.NatType;
        }
    }
}
