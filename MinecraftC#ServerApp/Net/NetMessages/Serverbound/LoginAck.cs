using DaisyCraft;
using NetMessages;

namespace Net.NetMessages.Serverbound
{
    [NetMetaTag(GameState.Login, 0x03)]
    public class LoginAck : INetMessage
    {
        public override void Handle(Connection connection, Server server)
        {
            connection.State = GameState.Configuration;
        }
    }
}
