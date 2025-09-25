using DaisyCraft;
using Net;
using Net.NetMessages.Clientbound;
using NetMessages.Serverbound;

namespace Net.NetMessages.Serverbound
{
    [PacketMetaData(GameState.Unknown, 0x00)]
    public class Handshake : ServerBoundPacket
    {
        [NetVarType(NetVarTypeEnum.Varint, 0)]
        public int Version { get; set; }

        [NetVarType(NetVarTypeEnum.String, 1)]
        public string Hostname { get; set; } = string.Empty;

        [NetVarType(NetVarTypeEnum.Uint16, 2)]
        public ushort Port { get; set; }
        
        // need a way to turn the number into a Enum again
        [NetVarType(NetVarTypeEnum.Byte, 3)]
        public GameState NextState { get; set; }


        public override async Task Handle(Player player, Server server) => player.State = NextState;
    }
}
