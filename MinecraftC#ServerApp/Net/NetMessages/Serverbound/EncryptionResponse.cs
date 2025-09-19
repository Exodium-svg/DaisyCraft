using DaisyCraft;
using DaisyCraft.Utils;
using Net.NetMessages.Clientbound;
using NetMessages;
using Scheduling;
using System.Security.Cryptography;
using System.Text.Json;
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

        public override async void Handle(Connection connection, Server server)
        {
            EncryptionRequestData? data = connection.Data as EncryptionRequestData;

            // some state is invalid? did we try to do a thing that isn't allowed?
            if (null == data)
            {
                connection.Close();
                return;
            }

            Challenge = data.Rsa.Decrypt(Challenge, RSAEncryptionPadding.Pkcs1);
            AesCipher = data.Rsa.Decrypt(AesCipher, RSAEncryptionPadding.Pkcs1);

            if ( !data.Token.SequenceEqual(Challenge) )
            {
                connection.Close(); // we failed to verify the token, close the connection.
                return;
            }

            // we will invalidate this, it's kind of bad as we are creating 3rd generation garbage, maybe use a structure? for passing it around my value instead of reference.
            connection.Data = null;

            // Enable encryption.
            connection.SetCipher(AesCipher);


            // need a type of usercache so we don't keep spamming this api...
            MojangApiResponse? response = await MojangApi.HasJoined(connection.Username, AesCipher, data.Rsa.ExportSubjectPublicKeyInfo());


            if ( null == response )
            {
                server.Logger.Warn($"Invalid session request from: {connection.Username} {connection.GetSocket().RemoteEndPoint}");
                connection.Send(new KickResponse("Invalid session"));
                server.GetService<Scheduler>().ScheduleDelayed(1000, () => { connection.Close(); return 0; });
                return;
            }

            int threshHold = server.Options.GetVar<int>("net.compression.thresh_hold", 127);

            connection.Send(new SetCompression(threshHold));
            connection.CompressionThreshold = threshHold;


            //connection.Send(new KickResponse("It works now"));
            connection.Send(new LoginSuccess(connection.UUID, connection.Username));

            //connection.State = GameState.Configuration;
            //connection.Send(new KickResponse("TODO: go to compression stage -> configuration state asflkjfasdlkjafsdjklasfdjklafsdjklasdfajklsfdjklkjlasdfkjlafsdjlkjkl"));

        }
    }
}
