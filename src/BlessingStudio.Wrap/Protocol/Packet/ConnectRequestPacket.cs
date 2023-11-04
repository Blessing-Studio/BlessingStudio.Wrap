using BlessingStudio.Wrap.Protocol.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlessingStudio.Wrap.Protocol.Packet
{
    public class ConnectRequestPacket : IPacket
    {
        [Field(0, ValueType.String)]
        public string UserToken = "";

        public PacketType GetPacketType()
        {
            return PacketType.ConnectRequest;
        }
    }
}
