using DaisyCraft;
using Net.NetMessages.Clientbound;
using NetMessages;
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

        public override void Handle(Connection connection, Server server)
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

            MojangApi.HasJoined(connection.Username, string.Empty, AesCipher, data.Rsa.ExportRSAPublicKey());




            connection.Send(new KickResponse("Hello do we work??"));


        }
    }
}
