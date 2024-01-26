using BlessingStudio.Wrap.Protocol.Attributes;

namespace BlessingStudio.Wrap.Protocol.Packet;

public sealed class LoginPacket : IPacket {
    [Field(1, ValueType.String)]
    public string UserToken = "_";

    [Field(0, ValueType.Bool)]
    public bool UseCustomToken = false;

    public PacketType GetPacketType() => PacketType.Login;
}