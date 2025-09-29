using Nbt.Tags;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace Nbt.Serialization
{

    public static class NbtWriteHelper
    {
        public static void WriteString(List<byte> buffer, string str)
        {
            buffer.AddRange(BitConverter.GetBytes(SwapEndian<short>((short)str.Length)));
            buffer.AddRange(Encoding.UTF8.GetBytes(str));
        }
        public static void WriteTag(List<byte> buffer, TagType type) => buffer.Add((byte)type);
        public static void WriteShort(List<byte> buffer, short value) => buffer.AddRange(BitConverter.GetBytes(SwapEndian<short>(value)));
        public static void WriteInt(List<byte> buffer, int value) => buffer.AddRange(BitConverter.GetBytes(SwapEndian<int>(value)));
        public static void WriteLong(List<byte> buffer, long value) => buffer.AddRange(BitConverter.GetBytes(SwapEndian<long>(value)));
        public static void WriteTags(List<byte> buffer, IEnumerable<INbtTag> tags)
        {
            foreach (var tag in tags)
            {
                if (tag.Value == null)
                    continue;

                WriteTag(buffer, tag.Type);
                WriteString(buffer, tag.Key);

                switch (tag.Type)
                {
                    case TagType.String:
                        WriteString(buffer, (string)tag.Value);
                        break;
                    case TagType.Byte:
                        buffer.Add((byte)tag.Value);
                        break;
                    case TagType.Short:
                        WriteShort(buffer, (short)tag.Value);
                        break;
                    case TagType.Int:
                        WriteInt(buffer, (int)tag.Value);
                        break;
                    case TagType.Long:
                        WriteLong(buffer, (long)tag.Value);
                        break;
                    case TagType.ByteArray:
                        byte[] byteArray = (byte[])tag.Value;
                        WriteInt(buffer, byteArray.Length);
                        buffer.AddRange(byteArray);
                        break;
                    case TagType.IntArray:
                        int[] intArray = (int[])tag.Value;
                        WriteInt(buffer, intArray.Length);

                        foreach (int i in intArray)
                            WriteInt(buffer, i);
                        break;
                    case TagType.LongArray:
                        long[] longArray = (long[])tag.Value;
                        WriteLong(buffer, longArray.Length);
                        foreach (long l in longArray)
                            WriteLong(buffer, l);
                        break;
                }
            }
        }
            
        public static T SwapEndian<T>(T value) where T : struct, INumber<T>
        {
            int size = Marshal.SizeOf<T>();

            if (size == 1)
                return value; // 1 byte has no endian difference

            byte[] bytes = new byte[size];

            if (typeof(T) == typeof(short))
                bytes = BitConverter.GetBytes((short)(object)value);
            else if (typeof(T) == typeof(ushort))
                bytes = BitConverter.GetBytes((ushort)(object)value);
            else if (typeof(T) == typeof(int))
                bytes = BitConverter.GetBytes((int)(object)value);
            else if (typeof(T) == typeof(uint))
                bytes = BitConverter.GetBytes((uint)(object)value);
            else if (typeof(T) == typeof(long))
                bytes = BitConverter.GetBytes((long)(object)value);
            else if (typeof(T) == typeof(ulong))
                bytes = BitConverter.GetBytes((ulong)(object)value);
            else if (typeof(T) == typeof(float))
                bytes = BitConverter.GetBytes((float)(object)value);
            else if (typeof(T) == typeof(double))
                bytes = BitConverter.GetBytes((double)(object)value);
            else if (typeof(T) == typeof(decimal))
                throw new NotSupportedException("SwapEndian not implemented for decimal.");
            else
                throw new NotSupportedException($"SwapEndian not supported for type {typeof(T)}");

            Array.Reverse(bytes); // swap bytes

            //object result = size switch
            //{
            //    2 when typeof(T) == typeof(short) => BitConverter.ToInt16(bytes, 0),
            //    2 when typeof(T) == typeof(ushort) => BitConverter.ToUInt16(bytes, 0),
            //    4 when typeof(T) == typeof(int) => BitConverter.ToInt32(bytes, 0),
            //    4 when typeof(T) == typeof(uint) => BitConverter.ToUInt32(bytes, 0),
            //    4 when typeof(T) == typeof(float) => BitConverter.ToSingle(bytes, 0),
            //    8 when typeof(T) == typeof(long) => BitConverter.ToInt64(bytes, 0),
            //    8 when typeof(T) == typeof(ulong) => BitConverter.ToUInt64(bytes, 0),
            //    8 when typeof(T) == typeof(double) => BitConverter.ToDouble(bytes, 0),
            //    _ => throw new NotSupportedException($"SwapEndian not supported for type {typeof(T)}"),
            //};

            

            return MemoryMarshal.Read<T>(bytes);
        }
    }
}

