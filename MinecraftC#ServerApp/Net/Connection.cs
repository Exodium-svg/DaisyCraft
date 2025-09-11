using Net;
using Net.NetMessages;
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
        public UInt128 UUID { get; set; } = UInt128.Zero;

        public object? Data { get; set; } = null;
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
            Aes.KeySize = cipherKey.Length * 8;
            Aes.FeedbackSize = 8;
            Aes.Key = cipherKey;
            Aes.IV = (byte[])cipherKey.Clone();

            Stream stream = client.GetStream();
            readStream = new CryptoStream(stream, Aes.CreateDecryptor(), CryptoStreamMode.Read);
            writeStream = new CryptoStream(stream, Aes.CreateEncryptor(), CryptoStreamMode.Write);
        }
        public void Send<T>(T msg) where T : class
        {
            byte[] payload = NetSerialization.Serialize<T>(msg);
            byte[] sizePrefix = Leb128.CreateVarInt(payload.Length).ToArray();

            byte[] packet = new byte[sizePrefix.Length + payload.Length];

            sizePrefix.CopyTo(packet, 0);
            payload.CopyTo(packet, sizePrefix.Length);

            try { GetWriteStream().Write(packet); } 
            catch(Exception) { } // we ignore exceptions, as this will set the connection as disconnected anyways.
        }

    }
}
