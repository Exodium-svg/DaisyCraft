using DaisyCraft;
using NetMessages;

namespace Net.NetMessages.Serverbound
{
    [NetMetaTag(GameState.Login, 0x03)]
    public class LoginAck : INetMessage
    {
        public override async Task Handle(Player player, Server server)
        {
            player.State = GameState.Configuration;
        }
        
    }
}
