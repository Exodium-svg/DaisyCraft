using Net;
using Net.NetMessages;

namespace Net.NetMessages.Clientbound.Login
{
    [PacketMetaData(GameState.Login, 0x03)]
    public class SetCompression
    {
        [NetVarType(NetVarTypeEnum.Varint, 0)]
        public int Threshold { get; init; }

        public SetCompression(int threshold) => Threshold = threshold;
        
    }
}
