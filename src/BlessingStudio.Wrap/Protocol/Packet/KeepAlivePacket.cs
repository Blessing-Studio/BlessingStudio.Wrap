using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlessingStudio.Wrap.Protocol.Packet
{
    public class KeepAlivePacket : IPacket
    {
        public PacketType GetPacketType()
        {
            return PacketType.KeepAlive;
        }
    }
}
