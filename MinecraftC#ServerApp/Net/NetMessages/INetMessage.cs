using DaisyCraft;
using Net;

namespace NetMessages
{
    public class INetMessage
    {
        public virtual void Handle(Connection connection, Server server)
        {
            // can do some shit here like messages handled.
        }
    }
}
