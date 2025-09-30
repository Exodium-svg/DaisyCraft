using Nbt.Tags;
using System.Buffers.Binary;
using System.Text;

namespace Nbt.Serialization
{
    public class NbtReader
    {
        private readonly Stream stream;
        private readonly bool isNetwork;
        public NbtReader(Stream stream, bool isNetwork)
        {
            this.stream = stream;
            this.isNetwork = isNetwork;
        }

        public bool Read(out NbtCompound tag)
        {
            TagType type = (TagType)stream.ReadByte();

            if (TagType.Compound != type)
            {
                tag = new NbtCompound("INVALID");
                return false;
            }

            if(isNetwork)
                tag = new NbtCompound("NET_ROOT");
            else
            {
                string key = ReadString();

                tag = new NbtCompound(key);
            }

            ReadCompound(ref tag);

            return true;
        }

        private INbtTag ReadTag(TagType type, string key)
        {
            switch(type)
            {
                case TagType.Compound:
                    NbtCompound tag = new NbtCompound(key);
                    ReadCompound(ref tag);
                    return tag;
                case TagType.ByteArray:
                    NbtNumericList<byte> byteArray = new(key);
                    byteArray.AddRange(ReadByteArray());
                    return byteArray;
                case TagType.IntArray:
                    NbtNumericList<int> intArray = new(key);
                    intArray.AddRange(ReadIntArray());
                    return intArray;
                case TagType.LongArray:
                    NbtNumericList<long> longArray = new(key);
                    longArray.AddRange(ReadLongArray());
                    return longArray;
                case TagType.List:
                    return ReadList(key);
                case TagType.String:
                    return new NbtTag<string>(key, ReadString());
                case TagType.Byte:
                    return new NbtTag<byte>(key, (byte)stream.ReadByte());
                case TagType.Short:
                    return new NbtTag<short>(key, ReadShort());
                case TagType.Int:
                    return new NbtTag<int>(key, ReadInt());
                case TagType.Long:
                    return new NbtTag<long>(key, ReadLong());
                case TagType.Float:
                    return new NbtTag<float>(key, ReadFloat());
                case TagType.Double:
                    return new NbtTag<double>(key, ReadDouble());
                default:
                    throw new NotSupportedException($"Tag ({key}) of type {type} is unsupported!");
            }
        }

        private INbtTag ReadList(string key)
        {
            TagType elementType = ReadTag();
            int count = ReadInt();

            NbtGenericList nbtList = new(key, elementType, count);

            for(int i = 0; i < count; i++)
                nbtList.Add(ReadTag(elementType, string.Empty));

            return nbtList;
        }
        private byte[] ReadByteArray()
        {
            int size = ReadInt();

            byte[] bytes = new byte[size];
            stream.ReadExactly(bytes);

            return bytes;
        }
        private int[] ReadIntArray()
        {
            int size = ReadInt();
            int[] ints = new int[size];

            for (int i = 0; i < size; i++) ints[i] = ReadInt();

            return ints;
        }

        private long[] ReadLongArray()
        {
            int size = ReadInt();
            long[] longs = new long[size];

            for(int i = 0; i < size; ++i) longs[i] = ReadLong();

            return longs;
        }

        private void ReadCompound(ref NbtCompound compoundTag)
        {
            while(true)
            {
                TagType type = ReadTag();
                if (TagType.End == type)
                    break;

                string key = ReadString();

                INbtTag tag = ReadTag(type, key);

                compoundTag[key] = tag;
            }
        }
        private string ReadString()
        {
            short size = ReadShort();
            
            Span<byte> buffer = size > 1024 ? new byte[size] : stackalloc byte[size];

            stream.ReadExactly(buffer);

            return Encoding.UTF8.GetString(buffer);
        }
        private TagType ReadTag()
        {
            int value = stream.ReadByte();

            return value == -1 ? TagType.End : (TagType)value; 
        }
        private short ReadShort()
        {
            Span<byte> buffer = stackalloc byte[sizeof(short)];

            stream.ReadExactly(buffer);
            return BinaryPrimitives.ReadInt16BigEndian(buffer);
        }
        private int ReadInt()
        {
            Span<byte> buffer = stackalloc byte[sizeof(int)];

            stream.ReadExactly(buffer);
            return BinaryPrimitives.ReadInt32BigEndian(buffer);
        }

        private long ReadLong()
        {
            Span<byte> buffer = stackalloc byte[sizeof(long)];

            stream.ReadExactly(buffer);
            return BinaryPrimitives.ReadInt64BigEndian(buffer);
        }

        private float ReadFloat()
        {
            Span<byte> buffer = stackalloc byte[sizeof(float)];

            stream.ReadExactly(buffer);
            return BinaryPrimitives.ReadSingleBigEndian(buffer);
        }

        private double ReadDouble()
        {
            Span<byte> buffer = stackalloc byte[sizeof(double)];

            stream.ReadExactly(buffer);
            return BinaryPrimitives.ReadDoubleBigEndian(buffer);
        }
    }
}
