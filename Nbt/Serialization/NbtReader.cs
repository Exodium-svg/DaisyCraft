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
                    return new NbtByteArray(key, ReadByteArray());
                case TagType.IntArray:
                    return new NbtIntArray(key, ReadIntArray());
                case TagType.LongArray:
                    return new NbtLongArray(key, ReadLongArray());
                case TagType.List:
                    return ReadList(key);
                case TagType.String:
                    return new NbtString(key, ReadString());
                case TagType.Byte:
                    return new NbtByte(key, (byte)stream.ReadByte());
                case TagType.Short:
                    return new NbtShort(key, ReadShort());
                case TagType.Int:
                    return new NbtInt(key, ReadInt());
                case TagType.Long:
                    return new NbtLong(key, ReadLong());
                case TagType.Float:
                    return new NbtFloat(key, ReadFloat());
                case TagType.Double:
                    return new NbtDouble(key, ReadDouble());
                case TagType.End:
                    return new NbtEndTag();
                default:
                    throw new NotSupportedException($"Tag ({key}) of type {type} is unsupported!");
            }
        }

        private INbtTag ReadList(string key)
        {
            TagType elementType = ReadTag();
            int count = ReadInt();

            INbtTag[] nbtArray = new INbtTag[count];

            for (int i = 0; i < count; i++)
                nbtArray[i] = (ReadTag(elementType, string.Empty));

            return new NbtList(key, elementType, nbtArray);
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
