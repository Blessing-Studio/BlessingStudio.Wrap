using BlessingStudio.Wrap.Protocol.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlessingStudio.Wrap.Protocol.Packet
{
    public class LoginSuccessfulPacket : IPacket
    {
        [Field(0, ValueType.String)]
        public string UserToken = "";
        [Field(1, ValueType.ByteArray)]
        public byte[] IPAddress = new byte[4];
        [Field(2, ValueType.Int32)]
        public int port = 0;
        public PacketType GetPacketType()
        {
            return PacketType.LoginSuccessful;
        }
    }
}
