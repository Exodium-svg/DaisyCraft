using Net;
using Net.NetMessages;
using System.IO.Compression;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace Net
{
    public enum GameState : byte
    {
        Unknown,
        Status,
        Login,
        // Yes these 2 fields are meant to be the same, Love mc protocol.
        Play,
        Transfer = 3,
    }
    public class Connection
    {
        readonly TcpClient client;
        public GameState State { get; set; }
        public ulong Id { get; set; } // Zero will indicate an unassigned id, meaning we have not completed the login process.
        public long LastPing { get; set; } = 0;
        public string Username { get; set; } = "Unknown user";
        public Guid UUID { get; set; } = Guid.Empty;

        public object? Data { get; set; } = null;
        public int CompressionThreshold { get; set; } = -1; // 0 means no compression.
        Aes? Aes { get; set; } = null;

        Stream readStream;
        Stream writeStream;
        public Connection(TcpClient client)
        {
            this.client = client;
            this.Id = 0;
            this.readStream = client.GetStream();
            this.writeStream = readStream;
            State = GameState.Unknown;
        }
        public bool IsConnected() => client.Connected;
        public void Close()
        {
            if (client.Connected)
                client.Close();

            readStream.Dispose();
        }

        public bool DataAvailable() => client.Client.Available > 0;
        public Stream GetReadStream() => readStream; // cipher stream / compress stream later :moon: pls end me
        public Stream GetWriteStream() => writeStream;
        public Socket GetSocket() => client.Client;
        public void SetCipher(byte[] cipherKey)
        {
            Aes = Aes.Create();

            Aes.Padding = PaddingMode.None;
            Aes.Mode = CipherMode.CFB;
            Aes.KeySize = 128;
            Aes.FeedbackSize = 8;
            Aes.Key = cipherKey;
            Aes.IV = (byte[])cipherKey.Clone();

            Stream stream = client.GetStream();
            readStream = new CryptoStream(stream, Aes.CreateDecryptor(), CryptoStreamMode.Read);
            writeStream = new CryptoStream(stream, Aes.CreateEncryptor(), CryptoStreamMode.Write);
        }
        public void Send<T>(T msg) where T : class
        {
            // Step 1: Serialize payload (PacketID + Data)
            byte[] payload = NetSerialization.Serialize(msg);

            byte[] finalPacket;

            if (CompressionThreshold == -1)
            {
                // Compression disabled
                using MemoryStream packet = new();
                Leb128.WriteVarInt(packet, payload.Length);
                packet.Write(payload);
                finalPacket = packet.ToArray();
            }

            else if (payload.Length >= CompressionThreshold)
            {
                // Compressed
                using MemoryStream compressed = new();
                using (ZLibStream zStream = new(compressed, CompressionMode.Compress, true))
                {
                    zStream.Write(payload, 0, payload.Length);
                    zStream.Flush();
                }

                using MemoryStream packet = new();
                Leb128.WriteVarInt(packet, (int)compressed.Length + Leb128.SizeOfVarInt(payload.Length));
                Leb128.WriteVarInt(packet, payload.Length); // Data Length (uncompressed size)
                packet.Write(compressed.ToArray());

                finalPacket = packet.ToArray();
            }
            else
            {
                // --- Uncompressed case ---
                using MemoryStream packet = new();
                Leb128.WriteVarInt(packet, payload.Length + Leb128.SizeOfVarInt(0));
                Leb128.WriteVarInt(packet, 0); // Data Length = 0 (means uncompressed)
                packet.Write(payload);

                finalPacket = packet.ToArray();
            }

            try
            {
                Stream stream = GetWriteStream();
                stream.Write(finalPacket, 0, finalPacket.Length);
                stream.Flush();

                if (stream is CryptoStream cipherStream)
                    cipherStream.FlushFinalBlock();
            }
            catch (Exception) { }
        }

    }
}
