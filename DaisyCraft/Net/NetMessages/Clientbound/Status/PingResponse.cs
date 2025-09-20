using Net;
using Net.NetMessages;

namespace Net.NetMessages.Clientbound.Status
{
    [NetMetaTag(GameState.Status, 0x01)]
    public class PingResponse
    {
        [NetVarType(NetVarTypeEnum.Long, 0)]
        public long Time { get; init; }
    }
}
