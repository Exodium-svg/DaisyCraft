using System.Runtime.InteropServices;

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
        public static string ReadString(this Stream stream, int size)
        {
            Span<byte> buff = (size > 1024) ? new byte[size] : stackalloc byte[size];
            stream.ReadExactly(buff);

            return System.Text.Encoding.UTF8.GetString(buff);
        }
    }
}
