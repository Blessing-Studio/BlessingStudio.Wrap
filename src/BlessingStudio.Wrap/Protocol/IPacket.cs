using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlessingStudio.Wrap.Protocol
{
    public partial interface IPacket
    {
        public PacketType GetPacketType();
    }
}
