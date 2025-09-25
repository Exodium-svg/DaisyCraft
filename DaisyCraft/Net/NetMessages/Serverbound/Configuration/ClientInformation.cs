using DaisyCraft;
using NetMessages.Serverbound;

namespace Net.NetMessages.Serverbound.Configuration
{
    [PacketMetaData(GameState.Configuration, 0x00)]
    public class ClientInformation : ServerBoundPacket
    {
        [NetVarType(NetVarTypeEnum.String, 0)]
        public string Locale { get; set; }
        [NetVarType(NetVarTypeEnum.Byte, 1)]
        public byte ViewDistance { get; set; }
        [NetVarType(NetVarTypeEnum.Varint, 2)]
        public ChatMode ChatMode { get; set; }
        [NetVarType(NetVarTypeEnum.Bool, 3)]
        public bool ChatColors { get; set; }
        [NetVarType(NetVarTypeEnum.Byte, 4)]
        public SkinMask SkinMask { get; set; }
        [NetVarType(NetVarTypeEnum.Varint, 5)]
        public MainHand DominantHand { get; set; }
        [NetVarType(NetVarTypeEnum.Bool, 6)]
        public bool ProfanityFilter { get; set; }
        [NetVarType(NetVarTypeEnum.Bool, 7)]
        public bool AllowServerListing { get; set; }
        [NetVarType(NetVarTypeEnum.Varint, 8)]
        public ParticleStatus ParticleStatus { get; set; }

        public override async Task Handle(Player player, Server server)
        {
            player.Locale = Locale;
            player.ChatMode = ChatMode;
            player.SkinMask = SkinMask;
            player.DominantHand = DominantHand;
            player.ViewDistance = ViewDistance;
        }
    }
}
