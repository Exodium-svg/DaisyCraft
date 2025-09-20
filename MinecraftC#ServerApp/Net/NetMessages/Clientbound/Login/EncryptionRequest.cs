using Net;
using Net.NetMessages;
using System.Security.Cryptography;

namespace Net.NetMessages.Clientbound.Login
{
    class EncryptionRequestData
    {
        public RSA Rsa { get; set; }
        public byte[] Token { get; set; }
    }

    [NetMetaTag(GameState.Login, 0x01)]
    public class EncryptionRequest
    {
        [NetVarType(NetVarTypeEnum.String, 0)]
        public string ServerId { get; init; }
        [NetVarType(NetVarTypeEnum.ByteArray, 1)]
        public byte[] PublicKey { get; init; }
        [NetVarType(NetVarTypeEnum.ByteArray, 2)]
        public byte[] Challenge { get; init; }

        [NetVarType(NetVarTypeEnum.Bool, 3)]
        public bool ShouldAuthenticate { get; init; }

        public EncryptionRequest(string serverId, byte[] publicKey, byte[] challenge, bool shouldAuthenticate = true)
        {
            ServerId = serverId;
            PublicKey = publicKey;
            Challenge = challenge;
            ShouldAuthenticate = shouldAuthenticate;
        }
    }
}
