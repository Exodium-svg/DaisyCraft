using DaisyCraft;
using Net;
using Net.NetMessages.Clientbound;
using NetMessages;

namespace Net.NetMessages.Serverbound
{
    [NetMetaTag(GameState.Unknown, 0x00)]
    public class Handshake : INetMessage
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


        public override void Handle(Connection connection, Server server)
        {
            base.Handle(connection, server);
            // we do our shit here, this way we do not have to worry about concerns yippers
            connection.State = NextState;

            //if (NextState == GameState.Status)
            //    connection.Send(new StatusResponse { Response = server.Status });
            // send message?
        }
    }
}
