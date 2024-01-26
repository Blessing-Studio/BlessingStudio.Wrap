using BlessingStudio.Wrap.Protocol.Attributes;

namespace BlessingStudio.Wrap.Protocol.Packet;

public sealed class ConnectAcceptPacket : IPacket {
    [Field(0, ValueType.String)]
    public string UserToken = string.Empty;

    public PacketType GetPacketType() => PacketType.ConnectAccept;
}
