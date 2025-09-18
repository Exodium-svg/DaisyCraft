using DaisyCraft;

namespace TickableServices
{
    public class TickableService
    {
        protected Server server;
        public virtual void Start(Server server) => this.server = server;
        public virtual string GetServiceName() => string.Empty;
        public virtual void Tick(long deltaTime) { }
    }
}
