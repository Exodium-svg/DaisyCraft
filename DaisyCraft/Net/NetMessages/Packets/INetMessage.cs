using DaisyCraft;
using Net;

namespace NetMessages
{
    public class INetMessage
    {
        public virtual async Task Handle(Player player, Server server)
        {
            // can do some shit here like messages handled.
        }
    }
}
