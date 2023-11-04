using BlessingStudio.Wrap.Protocol.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlessingStudio.Wrap.Protocol.Packet
{
    public class LoginPacket : IPacket
    {
        [Field(0, ValueType.Bool)]
        public bool UseCustomToken = false;
        [Field(1, ValueType.String)]
        public string UserToken = "_";

        public PacketType GetPacketType()
        {
            return PacketType.Login;
        }
    }
}
