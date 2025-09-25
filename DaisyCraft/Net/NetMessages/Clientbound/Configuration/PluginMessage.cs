namespace Net.NetMessages.Clientbound.Configuration
{
    [PacketMetaData(GameState.Login, 0x01)]
    public class PluginMessage
    {
        [NetVarType(NetVarTypeEnum.Identifier, 0)]
        public Identifier Identifier { get; init; }

        public PluginMessage(string tag, byte[] data)
        {
            Identifier = new Identifier(tag, data);
        }
    }
}
