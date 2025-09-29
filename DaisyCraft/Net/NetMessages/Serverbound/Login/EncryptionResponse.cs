using Net.NetMessages.Clientbound.Login;
using System.Security.Cryptography;
using NetMessages.Serverbound;
using DaisyCraft;
using Scheduling;
using Utils;

namespace Net.NetMessages.Serverbound
{
    [PacketMetaData(GameState.Login, 0x01)]
    public class EncryptionResponse : ServerBoundPacket
    {
        [NetVarType(NetVarTypeEnum.ByteArray, 0)]
        public byte[] AesCipher { get; set; }
        [NetVarType(NetVarTypeEnum.ByteArray, 1)]
        public byte[] Challenge { get; set; }

        public override async Task Handle(Player player, Server server)
        {
            EncryptionRequestData? data = player.Data as EncryptionRequestData;

            // some state is invalid? did we try to do a thing that isn't allowed?
            if (null == data)
            {
                player.Connection.Close();
                return;
            }

            Challenge = data.Rsa.Decrypt(Challenge, RSAEncryptionPadding.Pkcs1);
            AesCipher = data.Rsa.Decrypt(AesCipher, RSAEncryptionPadding.Pkcs1);

            if ( !data.Token.SequenceEqual(Challenge) )
            {
                player.Connection.Close(); // we failed to verify the token, close the connection.
                return;
            }

            player.Data = null;
            player.SetCipher(AesCipher);

            // need a type of usercache so we don't keep spamming this api...
            MojangApiResponse? response = await MojangApi.HasJoined(player.Username, AesCipher, data.Rsa.ExportSubjectPublicKeyInfo());

            if ( null == response )
            {
                server.Logger.Warn($"Invalid session request from: {player.Username} {player.Connection.RemoteEndPoint}");
                await player.Kick("Invalid session", server.GetService<Scheduler>());
                
                return;
            }

            player.Authenticated = true;

            await player.SetCompression(server.Options.GetVar<int>("net.compression.threshold", 127));
            await player.SendAsync(new LoginSuccess(player.Uuid, player.Username));

        }
    }
}
