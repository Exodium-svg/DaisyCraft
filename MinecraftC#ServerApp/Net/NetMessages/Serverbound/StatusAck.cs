using DaisyCraft;
using Net.NetMessages.Clientbound;
using NetMessages;
using System.Text.Json;

namespace Net.NetMessages.Serverbound
{
    [NetMetaTag(GameState.Status, 0x00)]
    public class StatusAck : INetMessage
    {
        public override void Handle(Connection connection, Server server)
        {
            base.Handle(connection, server);
            connection.State = GameState.Status;
            
            connection.Send(new StatusResponse { Response = JsonSerializer.Serialize(server.Status, new JsonSerializerOptions() { WriteIndented = false}) });
        }
    }
}
