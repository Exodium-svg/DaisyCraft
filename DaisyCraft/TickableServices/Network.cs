using DaisyCraft;
using DaisyCraft.Utils;
using Net;
using Net.NetMessages;
using Net.NetMessages.Packets;
using NetMessages.Serverbound;
using Scheduling;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Utils;

namespace TickableServices
{
    public class NetEventHolder
    {
        public Lock TransactionLock { get; init; } = new();
        public Player Player { get; init; }
        public byte[] TcpBuffer { get; init; }
        public byte[]? TrailingBuffer { get; set; } = null;
        public bool ReceivingPayload { get; set; } = false;
        //public NetPhase Phase { get; set; } = NetPhase.Header;

        public NetEventHolder(Player player, byte[] buffer)
        {
            Player = player;
            TcpBuffer = buffer;
        }
    }
    public class Network : TickableService
    {
        public const int PROTOCOL_VERSION = 773;
        const int MAX_PACKET_SIZE = 200000;

        List<Player> players = new List<Player>();

        bool isRunning = true;

        ConcurrentDictionary<GameState, Dictionary<int, Type>> packetHandlers = new();

        public string Address { get; init; }
        public int Port { get; init; }
        public List<IPAddress> BannedAddressList { get; init; } = new();
        Logger logger;

        Socket listener;
        ArrayPool<byte> bufferPool = ArrayPool<byte>.Shared;

        ConcurrentQueue<NetBuffer> backBuffer = new();
        ConcurrentQueue<NetBuffer> readBuffer = new();
        ConcurrentQueue<NetBuffer> receiveBuffer = new();

        public Network(string address, int port, Logger logger, Assembly packetAssembly, IEnumerable<string>? bannedIps = null)
        {
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
            IEnumerable<Type> netMsgTypes = packetAssembly.GetTypes().Where((t) => t.IsClass == true && t.GetCustomAttribute<PacketMetaData>() != null && typeof(ServerBoundPacket).IsAssignableFrom(t));


            if (netMsgTypes.Count() == 0)
            {
                logger.Error("No packet types found in the provided assembly. Network initialization failed.");
                throw new InvalidOperationException("No packet types found in the provided assembly.");
            }

            foreach (Type netMsgType in netMsgTypes)
            {
                // checked above we know it works
                var tag = netMsgType.GetCustomAttribute<PacketMetaData>()!;

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

                player.Connection.ReceiveTimeout = server.Options.GetVar<int>("net.timeout", 1000);
                player.Connection.SendTimeout = server.Options.GetVar<int>("net.timeout", 1000);

                lock (players)
                    players.Add(player);

                SocketAsyncEventArgs socketAsyncEventArgs = new SocketAsyncEventArgs();
                NetEventHolder eventHolder = new NetEventHolder(player, new byte[1024]);
                socketAsyncEventArgs.UserToken = eventHolder;
                socketAsyncEventArgs.SetBuffer(eventHolder.TcpBuffer);
                socketAsyncEventArgs.Completed += OnReceive;
                
                var pending = remoteSocket.ReceiveAsync(socketAsyncEventArgs);

                if( false == pending )
                    _ = Task.Run(() => OnReceive(null, socketAsyncEventArgs));
                
            }
        }
        private NetBuffer ParsePacket(Player player, ReadOnlySpan<byte> data, ref int offset, int size)
        {
            ReadOnlySpan<byte> offsetBuffer = data.Slice(offset);
            //int size = Leb128.ReadVarInt(offsetBuffer, out int varIntOffset);
            
            //offset += offset;
            
            NetBuffer buffer = new NetBuffer(player, bufferPool) { Compressed = player.CompressionEnabled };
            buffer.Reserve(size);
            data.Slice(offset, size).CopyTo((buffer.Buffer));
            
            offset += size;

            return buffer;
        }
        private void OnReceive(object? sender, SocketAsyncEventArgs args)
        {
            
            NetEventHolder eventHolder = (NetEventHolder)args.UserToken!;
            Player player = eventHolder.Player;

            lock (eventHolder.TransactionLock)
            {
                
                Span<byte> socketBuffer = eventHolder.TcpBuffer.AsSpan().Slice(0, args.BytesTransferred);

                if (!player.Connected)
                {
                    player.Connection.Close();
                    player.Connection.Dispose();
                    return;
                }

                if (args.SocketError != SocketError.Success || args.BytesTransferred <= 0)
                {
                    return;
                    if (player.Connected)
                        _ = player.Kick("Socket error", server.GetService<Scheduler>(), 1000);
                    else
                        player.Connection.Close();

                    return; // stop receiving client is dead.
                }
                
                Span<byte> packet;
                if (player.CipherEnabled)
                    packet = player.ReceiveCipher!.Decrypt(socketBuffer, 0, args.BytesTransferred);
                else
                    packet = socketBuffer;     

                try
                {
                    int bytesRead = 0;
                    while (bytesRead < args.BytesTransferred)
                    {
                        
                        if( false == Leb128.IsValidVarInt(packet.Slice(bytesRead))) // broken and fucked to do fix this
                        {
                            // mangled packet OR not complete varint which is out of bounds.
                            throw new Exception(" TODO IMPLEMENT ME ");
                        }


                        // validate if our size fits in our buffer otherwise we cache it and do it again
                        int size = Leb128.ReadVarInt(packet.Slice(bytesRead), ref bytesRead);

                        if( args.BytesTransferred < size )
                        {
                            // special case where we need to store state as we didn't receive things past the header.
                            throw new Exception(" TODO IMPLEMENT ME ");
                        }


                        NetBuffer buff = ParsePacket(player, packet, ref bytesRead, size);
                        
                        // make a case somehow for missing packet data as not fully received.

                        receiveBuffer.Enqueue(buff);
                    }

                    //ParseData(player, buffer, eventHolder, args); 
                }

                catch (SocketException) { }
                catch (Exception e) { server.Logger.Exception(e); player.Connection.Close(); } // problematic exception of any kind disconnects.


            }
            if ( false == player.Connected ) 
                return;// we might disconnect the client during the parsing phase.
            
            bool pending = player.Connection.ReceiveAsync(args);
            
            if( false == pending )
                OnReceive(null, args);
        }

        public override string GetServiceName() => nameof(Network);
        
        private Task HandlePacket(int packetId, Stream stream, Player player, NetBuffer buffer)
        { 
            Type packetType = packetHandlers[player.State][packetId];


            ServerBoundPacket? netMessage = Activator.CreateInstance(packetType) as ServerBoundPacket;


            if (null == netMessage)
            {
                string errMsg = $"Failed to create instance of packet ID {packetId} in state {player.State} from {player.Id}, connection closed.";
                logger.Error(errMsg);

                _ = player.Kick(errMsg, server.GetService<Scheduler>());
                return Task.CompletedTask;
            }

            NetSerialization.Deserialize(netMessage, packetId, stream);
            buffer.Dispose();

            return netMessage.Handle(player, server);
        }
        private Task ReadCompressed(NetBuffer buffer)
        {
            Player player = buffer.Owner;
            int readBytes = 0;
            int uncompressedLength = Leb128.ReadVarInt(buffer.Buffer, ref readBytes);

            byte[] packet;
            if (uncompressedLength != 0)
                packet = ZlibHelper.Decompress(buffer.Buffer.AsSpan().Slice(uncompressedLength).ToArray());
            else
                packet = buffer.Buffer!;
            
            using MemoryStream stream = new MemoryStream(packet);
            stream.Position = readBytes;

            int id = Leb128.ReadVarInt(stream);

            return HandlePacket(id, stream, player, buffer);
        }
        private Task ReadUncompressed(NetBuffer buffer)
        {
            Player player = buffer.Owner; 
            using MemoryStream stream = buffer.GetStream();

            int packetId = Leb128.ReadVarInt(stream);

            return HandlePacket(packetId, stream, player, buffer);
        }
        public override void Tick(long deltaTime)
        {
            base.Tick(deltaTime);
            
            var oldReceive = Interlocked.Exchange(ref receiveBuffer, backBuffer);
            var oldRead = Interlocked.Exchange(ref readBuffer, oldReceive);

            backBuffer = oldRead;


            List<Task> tasksToComplete = new(readBuffer.Count);

            while(!readBuffer.IsEmpty)
            {
                if (!readBuffer.TryDequeue(out NetBuffer? buffer))
                    continue;

                if (buffer.Owner.State == GameState.Kicked || !buffer.Owner.Connected)
                    continue;

                try
                {
                    if (buffer.Compressed)
                        tasksToComplete.Add(ReadCompressed(buffer));
                    else
                        tasksToComplete.Add(ReadUncompressed(buffer));
                } catch(Exception e)
                {
                    logger.Exception(e);
                }
            }

            //TODO handle all exceptions here so we do not crash.
            Task waitTask = Task.WhenAll(tasksToComplete);

            lock (players)
                players.RemoveAll((player) => {

                    if(!player.Connected)
                    {
                        if(player.Id != 0)
                            server.OnPlayerLeave(player.Id);

                        return true;
                    }

                    return false;
                    });

            waitTask.Wait();
        }

        public override void Start(Server server)
        {
            base.Start(server);
            Listen();
        }
    }
}
