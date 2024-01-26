using BlessingStudio.Wrap.Protocol.Packet;

namespace BlessingStudio.Wrap.Protocol;

public enum PacketType {
    Login,
    LoginSuccessful,
    LoginFailed,
    PluginMessage,
    Disconnect,
    KeepAlive,
    ConnectRequest,
    ConnectAccept,
    IPInfo,
    ConnectSuccessfully,
    ConnectRequestInvalidated
}

public partial interface IPacket {
    public static IReadOnlyDictionary<PacketType, Type> Packets { get; } = new Dictionary<PacketType, Type> {
        [PacketType.Login] = typeof(LoginPacket),
        [PacketType.LoginSuccessful] = typeof(LoginSuccessfulPacket),
        [PacketType.LoginFailed] = typeof(LoginFailedPacket),
        [PacketType.PluginMessage] = typeof(PluginMessagePacket),
        [PacketType.Disconnect] = typeof(DisconnectPacket),
        [PacketType.KeepAlive] = typeof(KeepAlivePacket),
        [PacketType.ConnectRequest] = typeof(ConnectRequestPacket),
        [PacketType.ConnectAccept] = typeof(ConnectAcceptPacket),
        [PacketType.IPInfo] = typeof(IPInfoPacket),
        [PacketType.ConnectSuccessfully] = typeof(ConnectSuccessfullyPacket),
        [PacketType.ConnectRequestInvalidated] = typeof(ConnectRequestInvalidatedPacket)
    };
}