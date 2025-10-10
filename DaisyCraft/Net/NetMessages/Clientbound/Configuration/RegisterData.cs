using DaisyCraft.Game.Registry;
using Nbt.Tags;
using Net;
using Net.NetMessages;

namespace DaisyCraft.Net.NetMessages.Clientbound.Configuration
{
    [PacketMetaData(GameState.Configuration, 0x07)]
    public class RegisterData
    {
        [NetVarType(NetVarTypeEnum.Identifier, 0)]
        public Identifier RegistryId { get; set; }
        [NetVarType(NetVarTypeEnum.NbtComponent, 1)]
        public IEnumerable<NbtCompound> Data { get; set; }

        public RegisterData(Identifier registryId, IEnumerable<NbtCompound> data)
        {
            RegistryId = registryId;
            Data = data;
        }
    }
}
