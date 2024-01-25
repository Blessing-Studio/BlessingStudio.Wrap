using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlessingStudio.Wrap.Protocol.Packet
{
    public class ConnectSuccessfullyPacket : IPacket
    {
        public PacketType GetPacketType()
        {
            return PacketType.ConnectSuccessfully;
        }
    }
}
