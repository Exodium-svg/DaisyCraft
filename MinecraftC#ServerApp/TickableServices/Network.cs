using DaisyCraft;
using DaisyCraft.Utils;
using Net;
using Net.NetMessages;
using Net.NetMessages.Packets;
using NetMessages;
using Scheduling;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text.Json.Serialization;
using Utils;

namespace TickableServices
{
    public enum NetPhase
    {
        Header,
        Payload,
    }
    public class NetEventHolder
    {
        public NetBuffer Buffer { get; set; }
        public Player Player { get; init; }
        public int Position { get; set; } = 0;
        public int MsgSize { get; set; } = 0;
        public NetPhase Phase { get; set; } = NetPhase.Header;

        public NetEventHolder(NetBuffer netBuffer, Player player)
        {
            Buffer = netBuffer;
            Player = player;
        }
    }
    public class Network : TickableService
    {
        const int MAX_PACKET_SIZE = 200000;

        List<Player> players = new List<Player>();

        bool isRunning = true;

        ConcurrentDictionary<GameState, Dictionary<int, Type>> packetHandlers = new();

        public string Address { get; init; }
        public int Port { get; init; }
        public List<IPAddress> BannedAddressList { get; init; } = new();
        Logger logger;

        Socket listener;
        Server server;
        ArrayPool<byte> bufferPool = ArrayPool<byte>.Shared;

        ConcurrentQueue<NetBuffer> readBuffer = new ConcurrentQueue<NetBuffer>();
        ConcurrentQueue<NetBuffer> receiveBuffer = new ConcurrentQueue<NetBuffer>();

        public Network(string address, int port, Logger logger, Assembly packetAssembly, IEnumerable<string>? bannedIps = null)
        {
            this.server = server;
            Address = address;
            Port = port;
            this.logger = logger;

            foreach (string bannedAddress in bannedIps ?? Array.Empty<string>())
            {
                if (IPAddress.TryParse(address, out var ip))
                    this.BannedAddressList.Add(ip);
                else
                    logger.Warn($"Failed to parse banned IP address: {address}");
            }

            // Load all packet types from the assembly with correct game state ( rip cpu cycles / memory )
            IEnumerable<Type> netMsgTypes = packetAssembly.GetTypes().Where((t) => t.IsClass == true && t.GetCustomAttribute<NetMetaTag>() != null && typeof(INetMessage).IsAssignableFrom(t));


            if (netMsgTypes.Count() == 0)
            {
                logger.Error("No packet types found in the provided assembly. Network initialization failed.");
                throw new InvalidOperationException("No packet types found in the provided assembly.");
            }

            foreach (Type netMsgType in netMsgTypes)
            {
                // checked above we know it works
                var tag = netMsgType.GetCustomAttribute<NetMetaTag>()!;

                if (!packetHandlers.ContainsKey(tag.State))
                    packetHandlers[tag.State] = new Dictionary<int, Type>();

                if (!packetHandlers[tag.State].TryAdd(tag.Id, netMsgType))
                {
                    logger.Warn($"Duplicate packet ID {tag.Id} in state {tag.State} for type {netMsgType.FullName}, already registered as {packetHandlers[tag.State][tag.Id].FullName}, skipping.");
                    continue;
                }
            }

            listener = new Socket(SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(new IPEndPoint(IPAddress.Parse(address), port));
            }
            catch (Exception e)
            {
                logger.Exception(e);
                throw;
            }

            listener.Listen();
        }

        public async void Listen()
        {
            logger.Info($"Networking listening on: {Address}:{Port}");

            while(isRunning)
            {
                Socket remoteSocket = await listener.AcceptAsync();

                var remoteIP = ((IPEndPoint)remoteSocket.RemoteEndPoint!).Address;

                if (BannedAddressList.Contains(remoteIP))
                {
                    remoteSocket.Close();
                    continue;
                }

                else if (!remoteSocket.Connected)
                {
                    remoteSocket.Close();
                    continue;
                }
                    


                Player player = new Player(remoteSocket);

                lock (players)
                    players.Add(player);

                SocketAsyncEventArgs socketAsyncEventArgs = new SocketAsyncEventArgs();
                socketAsyncEventArgs.UserToken = new NetEventHolder(new NetBuffer(player, bufferPool), player);
                socketAsyncEventArgs.SetBuffer(new byte[1024]);
                socketAsyncEventArgs.Completed += OnReceive;


                remoteSocket.ReceiveAsync(socketAsyncEventArgs);
            }
        }
        private void HeaderPhase(NetEventHolder eventHolder, NetBuffer buffer, Player player, SocketAsyncEventArgs args)
        {
            int varIntOffset;

            ReadOnlySpan<byte> readingBuffer = args.Buffer.AsSpan().Slice(eventHolder.Position);
            int size = Leb128.ReadVarInt(readingBuffer, out varIntOffset);


            if (size > MAX_PACKET_SIZE) // Go frick yourself
            {
                _ = player.Kick("Illegal packet", server.GetService<Scheduler>());
                return;
            }

            eventHolder.MsgSize = size;



            if (args.BytesTransferred < eventHolder.MsgSize)
            {
                eventHolder.Position = args.BytesTransferred;
                eventHolder.Phase = NetPhase.Payload;
                // big enough?
                buffer.Reserve(eventHolder.MsgSize);
                readingBuffer.Slice(varIntOffset).CopyTo(buffer.Buffer!);
            }
            else
            {
                readingBuffer.Slice(varIntOffset).CopyTo(buffer.Buffer!);

                receiveBuffer.Enqueue(buffer);

                eventHolder.MsgSize = 0;
                eventHolder.Position = 0;
                eventHolder.Buffer = new NetBuffer(player, bufferPool) { Compressed = player.CompressionEnabled };
            }
        }

        private void PayloadPhase(NetEventHolder eventHolder, NetBuffer buffer, Player player, SocketAsyncEventArgs args)
        {
            int bytesLeft = eventHolder.MsgSize - eventHolder.Position;
            Buffer.BlockCopy(args.Buffer, eventHolder.Position, buffer.Buffer!, eventHolder.Position, bytesLeft);

            eventHolder.Phase = NetPhase.Header;
            eventHolder.MsgSize = 0;
            eventHolder.Position = 0;

            receiveBuffer.Enqueue(buffer);

            eventHolder.Buffer = new NetBuffer(player, bufferPool) { Compressed = player.CompressionEnabled };
        }

        // This function is an attack vector as we abuse the stack here, should be rewritten to use a while loop lol.
        private void ParseData(Player player, NetBuffer buffer, NetEventHolder eventHolder, SocketAsyncEventArgs args)
        {
            int bytesLeft = eventHolder.MsgSize - eventHolder.Position;

            bool handleTrailingBytes = false;

            if (bytesLeft < args.BytesTransferred || eventHolder.MsgSize < args.BytesTransferred)
                handleTrailingBytes = true;

            if (eventHolder.Phase == NetPhase.Header)
                HeaderPhase(eventHolder, buffer, player, args);
            else
                PayloadPhase(eventHolder, buffer, player, args);



            if(handleTrailingBytes)
            {
                // handle edge case.
                Span<byte> remainingBytes = args.Buffer.AsSpan().Slice(bytesLeft);

                eventHolder.Position = bytesLeft;

                ParseData(player, buffer, eventHolder, args);
            }

            // check if eventHolder needed less data then what is remaining otherwise parse data again.
        }
        private void OnReceive(object? sender, SocketAsyncEventArgs args)
        {
            NetEventHolder eventHolder = (NetEventHolder)args.UserToken!;

            Player player = eventHolder.Player;
            NetBuffer buffer = eventHolder.Buffer;

            if( !player.Connected )
            {
                player.Connection.Close();
                player.Connection.Dispose();
                return;
            }
                

            if(args.SocketError != SocketError.Success && args.BytesTransferred <= 0)
            {
                if (player.Connected)
                    _ = player.Kick("Socket error", server.GetService<Scheduler>(), 1000);
                else
                    player.Connection.Close();

                return; // stop receiving client is dead.
            }

            //if (null == args.Buffer)
            //    throw new Exception("Not allowed, no buffer set!");


            if (player.CipherEnabled)
            {
                byte[] deciphered = player.ReceiveCipher!.Decrypt(args.Buffer, 0, args.BytesTransferred);
                deciphered.CopyTo(args.Buffer, 0);
            }

            ParseData(player, buffer, eventHolder, args);

            if (player.Connected ) // we might disconnect the client during the parsing phase.
                player.Connection.ReceiveAsync(args);
        }

        public override string GetServiceName() => nameof(Network);
        
        private void HandlePacket(int packetId, Stream stream, Player player)
        {
            Type packetType = packetHandlers[player.State][packetId];


            INetMessage? netMessage = Activator.CreateInstance(packetType) as INetMessage;


            if (null == netMessage)
            {
                string errMsg = $"Failed to create instance of packet ID {packetId} in state {player.State} from {player.Id}, connection closed.";
                logger.Error(errMsg);

                _ = player.Kick(errMsg, server.GetService<Scheduler>());
            }

            NetSerialization.Deserialize(netMessage!, packetId, stream);

            netMessage.Handle(null, server);
        }
        private void ReadCompressed(NetBuffer buffer)
        {
            Player player = buffer.Owner;

            int uncompressedLength = Leb128.ReadVarInt(buffer.Buffer, out int readBytes);


            byte[] packet = ZlibHelper.Decompress(buffer.Buffer.AsSpan().Slice(uncompressedLength).ToArray());

            using MemoryStream stream = new MemoryStream(packet);

            int id = Leb128.ReadVarInt(stream);

            HandlePacket(id, stream, player);
        }

        private void ReadUncompressed(NetBuffer buffer)
        {
            Player player = buffer.Owner; 
            using MemoryStream stream = buffer.GetStream();

            int packetId = Leb128.ReadVarInt(stream);

            HandlePacket(packetId, stream, player);
        }
        public override void Tick(long deltaTime)
        {
            base.Tick(deltaTime);

            Interlocked.Exchange(ref receiveBuffer, readBuffer);

            while(!readBuffer.IsEmpty)
            {
                if (!readBuffer.TryDequeue(out NetBuffer? buffer))
                    continue;

                if (buffer.Compressed)
                    ReadCompressed(buffer);
                else
                    ReadUncompressed(buffer);
            }

        }

        public override void Start(Server server)
        {
            base.Start(server);
            Listen();
        }
    }
}
