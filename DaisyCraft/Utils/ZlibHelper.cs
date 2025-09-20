using System.IO.Compression;

namespace Utils
{
    public static class ZlibHelper
    {
        public static byte[] Compress(byte[] data, CompressionLevel level)
        {
            using MemoryStream output = new MemoryStream();
            using (ZLibStream dstream = new ZLibStream(output, level))
            {
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        // this method is shit I know...
        public static byte[] Decompress(byte[] data)
        {
            using MemoryStream input = new MemoryStream(data);
            using ZLibStream zstream = new ZLibStream(input, CompressionMode.Decompress);
            using MemoryStream output = new MemoryStream();
            zstream.CopyTo(output);
            return output.ToArray();
        }
    }
}
