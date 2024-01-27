using BlessingStudio.Wrap.Protocol.Attributes;

namespace BlessingStudio.Wrap.Protocol.Packet;

public sealed class PluginMessagePacket : IPacket {
    [Field(0, ValueType.String)]
    public string MessageName = "";

    [Field(1, ValueType.ByteArray)]
    public byte[] Data = new byte[0];

    public PacketType GetPacketType() => PacketType.PluginMessage;
}