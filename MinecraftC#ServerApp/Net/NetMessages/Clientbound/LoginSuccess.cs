namespace Net.NetMessages.Clientbound
{
    [NetMetaTag(GameState.Login, 0x02)]
    public class LoginSuccess
    {
        [NetVarType(NetVarTypeEnum.UUID, 0)]
        public Guid Uuid { get; init; }
        [NetVarType(NetVarTypeEnum.String, 1)]
        public string Username { get; init; }

        [NetVarType(NetVarTypeEnum.Varint, 2)]
        public int TempPrefixHolder { get; init; } = 0; // using this hack to skip the properties part for now

        public LoginSuccess(Guid uuid, string username)
        {
            Uuid = uuid;
            Username = username;
        }
    }
}
