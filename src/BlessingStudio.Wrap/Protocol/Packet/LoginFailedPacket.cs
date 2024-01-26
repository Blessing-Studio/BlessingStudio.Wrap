using BlessingStudio.Wrap.Protocol.Attributes;

namespace BlessingStudio.Wrap.Protocol.Packet;

public sealed class LoginFailedPacket : IPacket {
    [Field(0, ValueType.String)]
    public string Reason = "None";

    public PacketType GetPacketType() => PacketType.LoginFailed;
}