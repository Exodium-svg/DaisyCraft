using DaisyCraft;
using NetMessages.Serverbound;

namespace Net.NetMessages.Serverbound
{
    [PacketMetaData(GameState.Login, 0x03)]
    public class LoginAck : ServerBoundPacket
    {
        public override async Task Handle(Player player, Server server) => player.State = GameState.Configuration;
    }
}
