using DaisyCraft;
using Net.NetMessages.Clientbound.Configuration;
using NetMessages.Serverbound;
using System.Text;
using Scheduling;
using DaisyCraft.Game.Registry;
using Nbt.Tags;
using DaisyCraft.Net.NetMessages.Clientbound.Configuration;

namespace Net.NetMessages.Serverbound.Configuration
{
    [PacketMetaData(GameState.Configuration, 0x02)]
    public class PluginResponse : ServerBoundPacket
    {
        [NetVarType(NetVarTypeEnum.Identifier, 0)]
        public Identifier Identifier { get; set; }
        [NetVarType(NetVarTypeEnum.ByteArray, 1)]
        public byte[] Data { get; set; }
        public override async Task Handle(Player player, Server server)
        {
            string identifier = Identifier.ToString();
            switch (identifier)
            {
                case "brand:minecraft":

                    if(player.Brand != string.Empty)
                    {
                        await player.Kick("Duplicate brand response", server.GetService<Scheduler>());
                        return;
                    }

                    player.Brand = Identifier.ToString();
                    await player.SendAsync(new PluginMessage(Identifier, Encoding.UTF8.GetBytes("DaisyCraft")));

                    Identifier trimId = new Identifier("minecraft", "trim_material");
                    List<NbtCompound> trimMaterials = server.Registry.GetAll(trimId.Namespace);
                    
                    await player.SendAsync(new RegisterData(trimId, trimMaterials));

                    break;
            }
        }
    }
}
