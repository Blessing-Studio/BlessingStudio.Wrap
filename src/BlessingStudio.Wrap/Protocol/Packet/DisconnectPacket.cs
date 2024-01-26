using BlessingStudio.Wrap.Protocol.Attributes;

namespace BlessingStudio.Wrap.Protocol.Packet;

public sealed class DisconnectPacket : IPacket {
    [Field(0, ValueType.String)]
    public string Reason = "";

    public PacketType GetPacketType() => PacketType.Disconnect;
}