using Net;
using Net.NetMessages;

namespace Net.NetMessages.Clientbound.Login
{
    [PacketMetaData(GameState.Login, 0x00)]
    public class LoginKick
    {
        [NetVarType(NetVarTypeEnum.String, 0)]
        public string Reason { get; set; } = string.Empty;
        
        public LoginKick(string reason) => Reason = $"{{text: \"{reason}\"}}";
        
    }
}
