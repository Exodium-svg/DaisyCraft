namespace Net
{
    public static class Leb128
    {
        public static int ReadVarInt(Stream stream)
        {
            int numRead = 0;
            int result = 0;
            byte read;
            do
            {
                int readByte = stream.ReadByte();
                if (readByte == -1)
                    throw new EndOfStreamException("Stream ended while attempting to read VarInt.");
                read = (byte)readByte;
                int value = (read & 0b01111111);
                result |= (value << (7 * numRead));
                numRead++;
                if (numRead > 5)
                    throw new InvalidDataException("VarInt is too big");
            } while ((read & 0b10000000) != 0);
            return result;
        }

        public static void WriteVarInt(Stream stream, int value)
        {
            do
            {
                byte temp = (byte)(value & 0b01111111);
                // Note: >>> means that the sign bit is shifted with the rest of the number rather than being left alone
                value >>= 7;
                if (value != 0)
                {
                    temp |= 0b10000000;
                }
                stream.WriteByte(temp);
            } while (value != 0);
        }
        public static byte[] CreateVarInt(int value)
        {
            List<byte> bytes = new();
            do
            {
                byte temp = (byte)(value & 0b01111111);
                // Note: >>> means that the sign bit is shifted with the rest of the number rather than being left alone
                value >>= 7;
                if (value != 0)
                {
                    temp |= 0b10000000;
                }
                bytes.Add(temp);
            } while (value != 0);
            return bytes.ToArray();
        }
    }
}
