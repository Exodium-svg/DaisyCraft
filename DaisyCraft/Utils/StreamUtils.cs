using System.Runtime.InteropServices;
using System.Text;
using Net;

namespace Utils
{
    public static class StreamUtils
    {
        public static T Read<T>(this Stream stream) where T : struct
        {
            Span<byte> buff = stackalloc byte[Marshal.SizeOf<T>()];
            stream.ReadExactly(buff);

            return MemoryMarshal.Read<T>(buff);
        }

        public static void Write<T>(this Stream stream, T value) where T : struct
        {
            Span<byte> buff = stackalloc byte[Marshal.SizeOf<T>()];
            MemoryMarshal.Write(buff, value);
            stream.Write(buff);
        }
        public static void WriteVarInt(this Stream stream, int value) => Leb128.WriteVarInt(stream, value);
        public static int ReadVarInt(this Stream stream) => Leb128.ReadVarInt(stream);
        public static void WriteVarLong(this Stream stream, long value) => Leb128.WriteVarLong(stream, value);
        public static long ReadVarLong(this Stream stream) => Leb128.ReadVarLong(stream);
        public static string ReadString(this Stream stream)
        {
            int size = Leb128.ReadVarInt(stream);
            Span<byte> buff = (size > 1024) ? new byte[size] : stackalloc byte[size];
            stream.ReadExactly(buff);

            return System.Text.Encoding.UTF8.GetString(buff);
        }

        public static void WriteString(this Stream stream, string str)
        {
            ReadOnlySpan<byte> buffer = Encoding.UTF8.GetBytes(str);

            Leb128.WriteVarInt(stream, buffer.Length);
            stream.Write(buffer);
            
        }
    }
}
