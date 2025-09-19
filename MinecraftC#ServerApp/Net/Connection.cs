using Net;
using Net.NetMessages;
using System.IO.Compression;
using System.Net.Sockets;
using System.Security.Cryptography;
using Utils;

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
            client.NoDelay = true;
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
            writeStream.Dispose();
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

        private void WriteUncompressed(Stream stream, byte[] payload)
        {
            Leb128.WriteVarInt(stream, payload.Length);
            stream.Write(payload);

            //if (stream is CryptoStream cipherStream)
            //    cipherStream.FlushFinalBlock();
        }
        private void WriteCompressed(Stream stream, byte[] payload)
        {
            // size | size uncompressed ( 0 IF below thresh hold )
            int uncompressedSize = payload.Length;
            if (payload.Length >= CompressionThreshold)
            {

                byte[] compressedPayload = ZlibHelper.Compress(payload, CompressionLevel.Fastest);


                Leb128.WriteVarInt(stream, compressedPayload.Length + Leb128.SizeOfVarInt(uncompressedSize));
                Leb128.WriteVarInt(stream, uncompressedSize);
                stream.Write(compressedPayload);
            }
            else
            { 
                Leb128.WriteVarInt(stream, uncompressedSize + Leb128.SizeOfVarInt(0));
                Leb128.WriteVarInt(stream, 0);
                stream.Write(payload);

                //if (stream is CryptoStream cipherStream)
                //    cipherStream.FlushFinalBlock();
            }
        }

        public void Send<T>(T msg) where T : class
        {
            // Step 1: Serialize payload (PacketID + Data)
            byte[] payload = NetSerialization.Serialize(msg);

            using MemoryStream packet = new();
            if (CompressionThreshold == -1)
                WriteUncompressed(packet, payload);

            else
                WriteCompressed(packet, payload);

            byte[] finalPacket = packet.ToArray();

            try
            {
                Stream stream = GetWriteStream();
                stream.Write(finalPacket, 0, finalPacket.Length);

                //stream.Flush();

                if (stream is CryptoStream cipherStream)
                {
                    cipherStream.Flush();
                }
                    
                else
                    stream.Flush();
            }
            catch (SocketException) { }
            catch (Exception ex) { Console.WriteLine($"Something went wrong in our sending: {ex}"); }
        }

    }
}
