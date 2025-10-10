using DaisyCraft.Game.Registry;

namespace Net.NetMessages.Clientbound.Configuration
{
    [PacketMetaData(GameState.Login, 0x01)]
    public class PluginMessage
    {
        [NetVarType(NetVarTypeEnum.Identifier, 0)]
        public Identifier Identifier { get; init; }
        [NetVarType(NetVarTypeEnum.ByteArray, 1)]
        public byte[] Data { get; init; }

        public PluginMessage(Identifier identifier, byte[] data)
        {
            Identifier = identifier;
            Data = data;
        }
    }
}
