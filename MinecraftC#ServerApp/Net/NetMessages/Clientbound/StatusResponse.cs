namespace Net.NetMessages.Clientbound
{
    [NetMetaTag(GameState.Status, 0x00)]
    public class StatusResponse
    {
        [NetVarType(NetVarTypeEnum.String, 0)]
        public string Response { get; init; } = string.Empty;
    }
}
