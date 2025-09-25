using DaisyCraft;
using Net.NetMessages.Clientbound.Login;
using NetMessages.Serverbound;
using System.Security.Cryptography;

namespace Net.NetMessages.Serverbound
{
    [PacketMetaData(GameState.Login, 0x00)]
    public class LoginStart : ServerBoundPacket
    {
        [NetVarType(NetVarTypeEnum.String, 0)]
        public string Username { get; set; } = string.Empty;

        [NetVarType(NetVarTypeEnum.UUID, 1)]
        public Guid UUID { get; set; }

        public override async Task Handle(Player player, Server server)
        {
            player.Username = Username;
            player.Uuid = UUID;

            RSA rsa = RSA.Create(1024);
            EncryptionRequestData requestData = new EncryptionRequestData
            {
                Rsa = rsa,
                Token = RandomNumberGenerator.GetBytes(16)
            };

            player.Data = requestData;

            await player.SendAsync(new EncryptionRequest(string.Empty, rsa.ExportSubjectPublicKeyInfo(), requestData.Token, true));
        }
    }
}
