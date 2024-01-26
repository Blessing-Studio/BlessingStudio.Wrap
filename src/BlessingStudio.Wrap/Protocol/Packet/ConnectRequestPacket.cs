using BlessingStudio.Wrap.Protocol.Attributes;

namespace BlessingStudio.Wrap.Protocol.Packet;

public sealed class ConnectRequestPacket : IPacket {
    [Field(0, ValueType.String)]
    public string UserToken = "";

    public PacketType GetPacketType() => PacketType.ConnectRequest;
}