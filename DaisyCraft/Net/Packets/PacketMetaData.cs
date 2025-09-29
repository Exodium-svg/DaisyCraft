using Net;

namespace Net.NetMessages
{
    // Maybe name the packets to a resource and then map them in the Registery codec as a number?
    [AttributeUsage(AttributeTargets.Class)]
    public class PacketMetaData : Attribute
    {
        public GameState State { get; init; }
        public int Id { get; init; }

        public PacketMetaData(GameState state, int id)
        {
            State = state;
            Id = id;
        }
    }
}
