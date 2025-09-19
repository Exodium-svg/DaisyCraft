using DaisyCraft;
using Net;
using DaisyCraft.Utils;
using Net.NetMessages;
using NetMessages;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Net.NetMessages.Clientbound;
using System.IO.Compression;

namespace TickableServices
{
    public class Network : TickableService
    {
        readonly string address;
        readonly int port;

        readonly Logger logger;
        TcpListener listener;
        List<Connection> connections = new();
        List<IPAddress> bannedIps = new();

        bool isRunning = true;

        Dictionary<GameState, Dictionary<int, Type>> packetHandlers = new();
        //readonly Action<Connection, Span<byte>> onMessage; // instead of this we should just pull the packet types out of the assembly and use reflection to call the right handler.
        public Network(string address, int port, Logger logger, Assembly packetAssembly, IEnumerable<string>? bannedIps = null)
        {
            this.address = address;
            this.port = port;
            this.logger = logger;

            foreach(string bannedAddress in bannedIps ?? Array.Empty<string>())
            {
                if (IPAddress.TryParse(address, out var ip))
                    this.bannedIps.Add(ip);
                else
                    logger.Warn($"Failed to parse banned IP address: {address}");
            }

            // Load all packet types from the assembly with correct game state ( rip cpu cycles / memory )
            IEnumerable<Type> netMsgTypes = packetAssembly.GetTypes().Where((t) => t.IsClass == true && t.GetCustomAttribute<NetMetaTag>() != null && typeof(INetMessage).IsAssignableFrom(t));


            if(netMsgTypes.Count() == 0)
            {
                logger.Error("No packet types found in the provided assembly. Network initialization failed.");
                throw new InvalidOperationException("No packet types found in the provided assembly.");
            }

            foreach (Type netMsgType in netMsgTypes)
            {
                // checked above we know it works
                var tag = netMsgType.GetCustomAttribute<NetMetaTag>()!;

                if(!packetHandlers.ContainsKey(tag.State))
                    packetHandlers[tag.State] = new Dictionary<int, Type>();

                if (!packetHandlers[tag.State].TryAdd(tag.Id, netMsgType))
                {
                    logger.Warn($"Duplicate packet ID {tag.Id} in state {tag.State} for type {netMsgType.FullName}, already registered as {packetHandlers[tag.State][tag.Id].FullName}, skipping.");
                    continue;
                }
            }

            listener = new TcpListener(IPAddress.Parse(address), port);
            
            try { listener.Start(); } // still throw here.
            catch(Exception ex) { logger.Exception(ex); throw; }
        }

        public async void Listen()
        {
            logger.Info($"Networking listening on: {address}:{port}");
            while (isRunning)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();

                IPEndPoint? endPoint = client.Client.RemoteEndPoint as IPEndPoint;

                if (null == endPoint || !client.Connected)
                {
                    client.Close();
                    continue;
                }

                if(bannedIps.Where((address) => address == endPoint.Address).Count() > 0)
                {
                    logger.Warn($"Rejected connection from banned IP: {endPoint.Address}");
                    client.Close();
                    continue;
                }

                
                lock(connections)
                    connections.Add(new Connection(client));
            }
        }
        private void HandleNetMessage(Connection connection, Stream stream, int packetId, List<Connection> toRemove)
        {
            if (!packetHandlers[connection.State].TryGetValue(packetId, out Type? netType))
            {
                logger.Error($"Received unknown packet ID {packetId} in state {connection.State} from {connection.Id}, connection closed.");

                connection.Close();
                toRemove.Add(connection);
                return;
            }

            INetMessage? netMsg = Activator.CreateInstance(netType) as INetMessage;

            if (null == netMsg)
            {
                string errMsg = $"Failed to create instance of packet ID {packetId} in state {connection.State} from {connection.Id}, connection closed.";
                logger.Error(errMsg);

                connection.Send(new KickResponse(errMsg));

                connection.Close();
                toRemove.Add(connection);
                return;
            }

            NetSerialization.Deserialize(netMsg, packetId, stream);

            // don't have server object yet, need to create.
            netMsg.Handle(connection, server);
        }
        private void HandlePacket(Connection connection, Stream stream, List<Connection> toRemove)
        {
            //Stream stream = connection.GetReadStream();

            try
            {
                //int size = Leb128.ReadVarInt(stream); // packet length, we don't actually need this.
                int packetId = Leb128.ReadVarInt(stream);
                
                HandleNetMessage(connection, stream, packetId, toRemove);
            }
            catch (EndOfStreamException)
            {
                connection.Close();
                if (!toRemove.Contains(connection))
                    toRemove.Add(connection);
            }
            catch (Exception ex)
            {
                logger.Exception(ex);
                connection.Close();
                if (!toRemove.Contains(connection))
                    toRemove.Add(connection);
                return;
            }
        }


        // we should be receiving async so we can be more scalable and then handle all the packets on a tick as they have already been received.
        public override void Tick(long deltaTime)
        {
            lock (connections)
            {
                List<Connection> toRemove = new();
                // should async foreach, 1 bad connection can block the rest. ( cuck code kill it )
                foreach (var connection in connections) // instead of doing this bull shit here, Make a queue of packets which we clear each tick.
                {
                    if (!connection.IsConnected())
                    {
                        connection.Close();
                        //connections.Remove(connection);
                        toRemove.Add(connection);
                        continue;
                    }

                    if (!connection.DataAvailable())
                        continue;
                    //var message = connection.ReadMessage();

                    Stream stream = connection.GetReadStream();

                    int length = Leb128.ReadVarInt(stream);

                    byte[] data = new byte[length];
                    stream.ReadExactly(data);

                    using MemoryStream ms = new(data);


                    if (length <= connection.CompressionThreshold)
                    {
                        // is compressed
                        int compressedLength = Leb128.ReadVarInt(ms); // we ignore this, as we just assume it's right because we are lazy pieces of shit.

                        using ZLibStream zStream = new ZLibStream(ms, CompressionMode.Decompress);

                        int packetId = Leb128.ReadVarInt(zStream);
                        HandleNetMessage(connection, zStream, packetId, toRemove);
                    }
                    else
                    {
                        HandlePacket(connection, ms, toRemove);
                        continue;
                    }
                }

                foreach (var rem in toRemove)
                    connections.Remove(rem);
            }
        }

        public override string GetServiceName() => nameof(Network);
        public override void Start(Server server)
        {
            base.Start(server);
            Listen();
        }
        
    }
}
