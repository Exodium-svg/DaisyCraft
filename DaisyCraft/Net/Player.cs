using Net.Cipher;
using Net.NetMessages;
using Net.NetMessages.Clientbound.Login;
using Scheduling;
using System.IO.Compression;
using System.Net.Sockets;
using Utils;

namespace Net
{
    public class Player
    {
        public Guid Uuid { get; set; } = Guid.Empty;
        public string Username { get; set; } = string.Empty;

        public ulong Id { get; set; } = 0;
        public GameState State { get; set; } = GameState.Unknown;
        public ulong LastPing { get; set; } = 0;

        public Socket Connection { get; init; }
        public bool Connected { get => Connection.Connected; }
        public AesCfbBlockCipher? SendCipher { get; private set; } = null;
        public AesCfbBlockCipher? ReceiveCipher { get; private set; } = null;
        public object? Data { get; set; } = null;

        public bool CipherEnabled { get; private set; } = false;
        public bool CompressionEnabled { get; private set; } = false;
        public int CompressionThreshold { get; private set; } = -1;

        public Player(Socket socket)
        {
            Connection = socket;
        }

        public void SetCipher(byte[] cipherKey)
        {
            SendCipher = new AesCfbBlockCipher(cipherKey);
            ReceiveCipher = new AesCfbBlockCipher(cipherKey);
            CipherEnabled = true;
        }

        public void SetCompression(int threshold)
        {
            CompressionThreshold = threshold;
            CompressionEnabled = true;
        }

        public async Task Kick(string reason, Scheduler scheduler, int disconnectDelay = 1000)
        {
            switch(State)
            {
                case GameState.Unknown:
                    break;
                case GameState.Status:
                    break;
                case GameState.Login:
                    await SendAsync(new KickResponse(reason));
                    break;
            }

            scheduler.ScheduleDelayed(disconnectDelay, () => { Connection.Close(); return 0; });
        }

        public async Task SendAsync<T>(T netMessage) where T : class
        {
            byte[] payload = NetSerialization.Serialize(netMessage);

            using var stream = new MemoryStream();

            if (CompressionEnabled)
                WriteCompressed(stream, payload);
            else
                WriteUncompressed(stream, payload);

            ReadOnlySpan<byte> packetSpan = new ReadOnlySpan<byte>(stream.GetBuffer(), 0, (int)stream.Length);

            
            byte[] finalPacket;

            // in both cases we take ownership over the byte array
            if (CipherEnabled)
                lock (SendCipher!)
                    finalPacket = SendCipher.Encrypt(packetSpan);
            else
                finalPacket = packetSpan.ToArray();

            try { await Connection.SendAsync(finalPacket, SocketFlags.None); }
            catch (Exception) { } // we ignore exceptions, socket class handles it already itself
        }

        #region Private methods
        private void WriteUncompressed(Stream stream, byte[] payload)
        {
            Leb128.WriteVarInt(stream, payload.Length);
            stream.Write(payload);
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
            }
        }
        #endregion
    }
}
