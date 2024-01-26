namespace BlessingStudio.Wrap.Protocol.Packet;

public sealed class ConnectSuccessfullyPacket : IPacket {
    public PacketType GetPacketType() => PacketType.ConnectSuccessfully;
}
