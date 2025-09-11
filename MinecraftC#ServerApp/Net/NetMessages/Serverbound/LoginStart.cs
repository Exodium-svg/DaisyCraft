using DaisyCraft;
using Net.NetMessages.Clientbound;
using NetMessages;
using System.Security.Cryptography;

namespace Net.NetMessages.Serverbound
{
    [NetMetaTag(GameState.Login, 0x00)]
    public class LoginStart : INetMessage
    {
        [NetVarType(NetVarTypeEnum.String, 0)]
        public string Username { get; set; } = string.Empty;

        [NetVarType(NetVarTypeEnum.UUID, 1)]
        public UInt128 UUID { get; set; }

        public override void Handle(Connection connection, Server server)
        {
            base.Handle(connection, server);

            connection.Username = Username;
            connection.UUID = UUID;

            //connection.Send(new KickResponse(Username));
            RSA rsa = RSA.Create(1024);
            EncryptionRequestData requestData = new EncryptionRequestData
            {
                Rsa = rsa,
                Token = RandomNumberGenerator.GetBytes(16)
            };

            connection.Data = requestData;

            connection.Send(new EncryptionRequest(string.Empty, rsa.ExportSubjectPublicKeyInfo(), requestData.Token, true));
        }
    }
}
