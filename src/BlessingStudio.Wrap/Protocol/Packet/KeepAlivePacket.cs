namespace BlessingStudio.Wrap.Protocol.Packet;

public sealed class KeepAlivePacket : IPacket {
    public PacketType GetPacketType() => PacketType.KeepAlive;
}
