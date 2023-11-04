using BlessingStudio.Wrap.Protocol.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlessingStudio.Wrap.Protocol.Packet
{
    public class PluginMessagePacket : IPacket
    {
        [Field(0, ValueType.String)]
        public string MessageName = "";
        [Field(1, ValueType.ByteArray)]
        public byte[] Data = Array.Empty<byte>();

        public PacketType GetPacketType()
        {
            return PacketType.PluginMessage;
        }
    }
}
