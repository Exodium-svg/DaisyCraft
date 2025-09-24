using DaisyCraft;
using Net;

namespace NetMessages.Serverbound
{
    public class ServerBoundPacket
    {
        public virtual async Task Handle(Player player, Server server)
        {
            // can do some shit here like messages handled.
        }
    }
}
