using NetMessages.Serverbound;
using System.Reflection;
using Utils;

namespace Net.NetMessages
{
    public static class NetSerialization
    {
        public static byte[] Serialize<T>(T obj) where T : class
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            Type type = obj.GetType();

            // Get message ID from NetMetaTag
            var metaTag = type.GetCustomAttribute<NetMetaTag>();
            if (metaTag == null)
                throw new InvalidDataException($"Type {type.FullName} is missing NetMetaTag attribute.");

            using MemoryStream ms = new MemoryStream();

            Leb128.WriteVarInt(ms, metaTag.Id); // Write message ID as VarInt

            var properties = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .OrderBy(p =>
                {
                    var tag = p.GetCustomAttribute<NetVarType>();
                    if (tag == null)
                        throw new InvalidDataException($"Property {p.Name} in type {type.FullName} is missing NetVarType attribute.");
                    return tag.Order;
                });

            foreach (var prop in properties)
            {
                var tag = prop.GetCustomAttribute<NetVarType>()!;
                object? value = prop.GetValue(obj);

                switch (tag.VarType)
                {
                    case NetVarTypeEnum.Byte:
                        ms.WriteByte((byte)value!);
                        break;

                    case NetVarTypeEnum.Varint:
                        Leb128.WriteVarInt(ms, (int)value!);
                        break;

                    case NetVarTypeEnum.String:
                        string str = (string)value!;
                        byte[] strBytes = System.Text.Encoding.UTF8.GetBytes(str);
                        Leb128.WriteVarInt(ms, strBytes.Length); // write length first
                        ms.Write(strBytes, 0, strBytes.Length);
                        break;

                    case NetVarTypeEnum.ByteArray:
                        byte[] byteArray = (byte[])value!;
                        Leb128.WriteVarInt(ms, byteArray.Length);
                        ms.Write(byteArray);
                        break;
                    case NetVarTypeEnum.Uint16:
                        ushort ushortVal = Convert.ToUInt16(value!);
                        ushortVal = (ushort)((ushortVal >> 8) | (ushortVal << 8)); // endian swap if needed
                        ms.Write<ushort>(ushortVal);
                        break;

                    case NetVarTypeEnum.UUID:
                        ms.Write(((Guid)value!).ToByteArray());
                        break;
                    case NetVarTypeEnum.Long:
                        ms.Write<long>((long)value!);
                        break;
                    case NetVarTypeEnum.Bool:
                        ms.Write((bool)value! == true ? (byte)1 : (byte)0);
                        break;
                    default:
                        throw new NotImplementedException($"Serialization for {tag.VarType} is not implemented.");
                }
            }

            return ms.ToArray();
        }
        
        // if too slow we can also turn this into code gen.
        public static void Deserialize(ServerBoundPacket netMsg, int msgId, Stream stream)
        {
            // do deserialization here.
            Type type = netMsg.GetType();

            var tag = type.GetCustomAttribute<NetMetaTag>();

            if (null == tag)
                throw new InvalidDataException($"Type {type.FullName} is missing NetMetaTag attribute.");

            if ( tag.Id != msgId)
                throw new InvalidDataException($"Message ID mismatch. Expected {tag.Id}, got {msgId}.");

            PropertyInfo[] properties = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .OrderBy(p =>
                {
                    var tag = p.GetCustomAttribute<NetVarType>();
                    if (tag == null)
                        throw new InvalidDataException($"Property {p.Name} in type {type.FullName} is missing NetVarType attribute.");
                    return tag.Order;
                }).ToArray();

            foreach (var prop in properties)
            {
                // we know that it is not null as we already checked that above.
                NetVarType fieldTag = prop.GetCustomAttribute<NetVarType>()!;
                
                switch(fieldTag.VarType)
                {
                    case NetVarTypeEnum.Byte:
                        prop.SetValue(netMsg, (byte)stream.ReadByte());
                    break;
                    case NetVarTypeEnum.Varint:
                        prop.SetValue(netMsg, Leb128.ReadVarInt(stream));
                    break;
                    case NetVarTypeEnum.String:
                        int length = Leb128.ReadVarInt(stream);
                        prop.SetValue(netMsg, stream.ReadString(length));
                        break;
                    case NetVarTypeEnum.ByteArray:
                        int byteLen = Leb128.ReadVarInt(stream);
                        byte[] bytes = new byte[byteLen];
                        stream.ReadExactly(bytes);
                        prop.SetValue(netMsg, bytes);
                        break;
                    case NetVarTypeEnum.Uint16:
                        ushort value = stream.Read<ushort>();
                        prop.SetValue(netMsg, (ushort)((value << 8) | (value >> 8)));
                        break;
                    case NetVarTypeEnum.UUID:
                        Span<byte> guidBytes = stackalloc byte[16];
                        stream.ReadExactly(guidBytes);

                        prop.SetValue(netMsg, new Guid(guidBytes));
                        break;
                    case NetVarTypeEnum.Long:
                        prop.SetValue(netMsg, stream.Read<long>());
                        break;
                    default:
                        throw new NotImplementedException($"Deserialization for {fieldTag.VarType} is not implemented.");

                }
            }
        }

    }
}
