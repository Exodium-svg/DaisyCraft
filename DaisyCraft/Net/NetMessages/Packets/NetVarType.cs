namespace Net.NetMessages
{
    public enum NetVarTypeEnum
    {
        Byte,
        Bool,
        Varint,
        Varlong,
        Long,
        Uint16,
        String,
        UUID,
        ByteArray,
        Identifier,
    }
    public class NetVarType : Attribute
    {
        public NetVarTypeEnum VarType { get; init; }
        public int Order { get; init; }
        
        public NetVarType(NetVarTypeEnum varType, int order)
        {
            VarType = varType;
            Order = order;
        }
    }
}
