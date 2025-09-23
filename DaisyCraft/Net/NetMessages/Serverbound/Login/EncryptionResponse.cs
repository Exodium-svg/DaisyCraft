using Net.NetMessages.Clientbound.Login;
using System.Security.Cryptography;
using NetMessages;
using DaisyCraft;
using Scheduling;
using Utils;

namespace Net.NetMessages.Serverbound
{
    [NetMetaTag(GameState.Login, 0x01)]
    public class EncryptionResponse : INetMessage
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

            // we will invalidate this, it's kind of bad as we are creating 3rd generation garbage, maybe use a structure? for passing it around my value instead of reference.
            player.Data = null;

            // Enable encryption.
            player.SetCipher(AesCipher);


            // need a type of usercache so we don't keep spamming this api...
            MojangApiResponse? response = await MojangApi.HasJoined(player.Username, AesCipher, data.Rsa.ExportSubjectPublicKeyInfo());


            if ( null == response )
            {
                server.Logger.Warn($"Invalid session request from: {player.Username} {player.Connection.RemoteEndPoint}");
                await player.Kick("Invalid session", server.GetService<Scheduler>());
                server.GetService<Scheduler>().ScheduleDelayed(1000, () => { player.Connection.Close(); return 0; });
                return;
            }

            int threshold = server.Options.GetVar<int>("net.compression.threshold", 127);

            await player.SendAsync(new SetCompression(threshold));

            player.SetCompression(threshold);


            //connection.Send(new KickResponse("It works now"));
            await player.SendAsync(new LoginSuccess(player.Uuid, player.Username));

            //connection.State = GameState.Configuration;
            //connection.Send(new KickResponse("TODO: go to compression stage -> configuration state asflkjfasdlkjafsdjklasfdjklafsdjklasdfajklsfdjklkjlasdfkjlafsdjlkjkl"));

        }
    }
}
