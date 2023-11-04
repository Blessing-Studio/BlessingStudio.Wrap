using BlessingStudio.Wrap.Protocol.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlessingStudio.Wrap.Protocol
{
    public enum PacketType
    {
        Login,
        LoginSuccessful,
        LoginFailed,
        PluginMessage,
        Disconnect,
        KeepAlive,
        ConnectRequest,
        ConnectAccept,
        IPInfo
    }
    public partial interface IPacket
    {
        public static Dictionary<PacketType, Type> Packets { get; set; } = new Dictionary<PacketType, Type>();
        static IPacket()
        {
            Packets[PacketType.Login] = typeof(LoginPacket);
            Packets[PacketType.LoginSuccessful] = typeof(LoginSuccessfulPacket);
            Packets[PacketType.LoginFailed] = typeof(LoginFailedPacket);
            Packets[PacketType.PluginMessage] = typeof(PluginMessagePacket);
            Packets[PacketType.Disconnect] = typeof(DisconnectPacket);
            Packets[PacketType.KeepAlive] = typeof(KeepAlivePacket);
            Packets[PacketType.ConnectRequest] = typeof(ConnectRequestPacket);
            Packets[PacketType.ConnectAccept] = typeof(ConnectAcceptPacket);
            Packets[PacketType.IPInfo] = typeof(IPInfoPacket);
        }
    }
}
