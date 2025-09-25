using DaisyCraft;
using Net.NetMessages.Clientbound.Configuration;
using NetMessages.Serverbound;
using System.Text;

namespace Net.NetMessages.Serverbound.Configuration
{
    [PacketMetaData(GameState.Configuration, 0x02)]
    public class PluginResponse : ServerBoundPacket
    {
        //[NetVarType(NetVarTypeEnum.String, 0)]
        //public string Identifier { get; set; }
        //[NetVarType(NetVarTypeEnum.ByteArray, 1)]
        //public byte[] Data { get; set; }
        [NetVarType(NetVarTypeEnum.Identifier, 0)]
        public Identifier Identifier { get; set; }
        public override async Task Handle(Player player, Server server)
        {
            switch(Identifier.Tag)
            {
                case "minecraft:brand":
                    player.Brand = Identifier.GetString();
                    await player.SendAsync(new PluginMessage("minecraft:brand", Encoding.UTF8.GetBytes("DaisyCraft")));
                    break;
            }
        }
    }
}
