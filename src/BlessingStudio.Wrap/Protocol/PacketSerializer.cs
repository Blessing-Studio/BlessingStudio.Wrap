using BlessingStudio.WonderNetwork.Extensions;
using BlessingStudio.WonderNetwork.Interfaces;
using BlessingStudio.Wrap.Protocol.Attributes;
using BlessingStudio.Wrap.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BlessingStudio.Wrap.Protocol
{
    public class PacketSerializer : ISerializer<IPacket>
    {
        public IPacket Deserialize(byte[] data)
        {
            using MemoryStream stream = new MemoryStream(data);
            PacketType packetType = (PacketType)stream.ReadByte();
            Type type = IPacket.Packets[packetType];
            List<Tuple<FieldAttribute, FieldInfo>> attributes = ReflectionUtils.FindFields(type);
            IPacket packet = (IPacket)Activator.CreateInstance(type)!;
            foreach (Tuple<FieldAttribute, FieldInfo> attribute in attributes)
            {
                FieldAttribute fieldAttribute = attribute.Item1;
                object? value = null;
                switch (fieldAttribute.valueType)
                {
                    case ValueType.String:
                        value = stream.ReadString();
                        break;
                    case ValueType.Int32:
                        value = stream.ReadInt32();
                        break;
                    case ValueType.VarInt:
                        value = stream.ReadVarInt();
                        break;
                    case ValueType.Int64:
                        value = stream.ReadInt64();
                        break;
                    case ValueType.Int16:
                        value = stream.ReadInt16();
                        break;
                    case ValueType.Byte:
                        value = (byte)stream.ReadByte();
                        break;
                    case ValueType.ByteArray:
                        value = new byte[stream.ReadVarInt()];
                        stream.Read((byte[])value);
                        break;
                    case ValueType.Bool:
                        {
                            byte b = (byte)stream.ReadByte();
                            value = b == 1;
                        }
                        break;
                    default:
                        break;
                }
                if (value != null)
                {
                    attribute.Item2.SetValue(packet, value);
                }
            }
            return packet;
        }

        public byte[] Serialize(IPacket @object)
        {
            Type type = @object.GetType();
            List<Tuple<FieldAttribute, FieldInfo>> attributes = ReflectionUtils.FindFields(type);
            using MemoryStream stream = new MemoryStream();
            stream.WriteByte((byte)@object.GetPacketType());
            foreach (Tuple<FieldAttribute, FieldInfo> attribute in attributes)
            {
                FieldAttribute fieldAttribute = attribute.Item1;
                object value = attribute.Item2.GetValue(@object)!;
                switch (fieldAttribute.valueType)
                {
                    case ValueType.String:
                        stream.WriteString((string)value);
                        break;
                    case ValueType.Int32:
                        stream.WriteInt32((int)value);
                        break;
                    case ValueType.VarInt:
                        stream.WriteVarInt((int)value);
                        break;
                    case ValueType.Int64: 
                        stream.WriteInt64((long)value);
                        break;
                    case ValueType.Int16:
                        stream.WriteInt16((short)value);
                        break;
                    case ValueType.Byte: 
                        stream.WriteByte((byte)value);
                        break;
                    case ValueType.ByteArray:
                        stream.WriteVarInt(((byte[])value).Length);
                        stream.Write((byte[])value);
                        break;
                    case ValueType.Bool:
                        if ((bool)value)
                        {
                            stream.WriteByte(0x01);
                        }
                        else
                        {
                            stream.WriteByte(0x01);
                        }
                        break;
                    default:
                        break;
                }
            }
            return stream.ToArray();
        }
    }
}
