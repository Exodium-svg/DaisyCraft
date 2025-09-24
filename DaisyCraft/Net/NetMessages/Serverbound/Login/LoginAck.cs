using DaisyCraft;
using NetMessages.Serverbound;

namespace Net.NetMessages.Serverbound
{
    [NetMetaTag(GameState.Login, 0x03)]
    public class LoginAck : ServerBoundPacket
    {
        public override async Task Handle(Player player, Server server)
        {
            server.Logger.Info("we are in config mode now!");
            player.State = GameState.Configuration;
        }
        
    }
}
