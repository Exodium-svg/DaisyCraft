using DaisyCraft;
using Net;
using Net.NetMessages;
using Net.NetMessages.Clientbound.Status;
using NetMessages.Serverbound;
using System.Text.Json;

namespace Net.NetMessages.Serverbound.Status
{
    [PacketMetaData(GameState.Status, 0x00)]
    public class StatusAck : ServerBoundPacket
    {
        public override async Task Handle(Player player, Server server)
        {
            player.State = GameState.Status;
            
            await player.SendAsync(new StatusResponse { Response = JsonSerializer.Serialize(server.Status, new JsonSerializerOptions() { WriteIndented = false}) });
        }
    }
}
