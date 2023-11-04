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
        }
    }
}
