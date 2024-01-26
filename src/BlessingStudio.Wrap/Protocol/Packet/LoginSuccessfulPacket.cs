using BlessingStudio.Wrap.Protocol.Attributes;

namespace BlessingStudio.Wrap.Protocol.Packet;

public sealed class LoginSuccessfulPacket : IPacket {
    [Field(2, ValueType.Int32)]
    public int port = 0;

    [Field(0, ValueType.String)]
    public string UserToken = "";

    [Field(1, ValueType.ByteArray)]
    public byte[] IPAddress = new byte[4];

    public PacketType GetPacketType() => PacketType.LoginSuccessful;
}
