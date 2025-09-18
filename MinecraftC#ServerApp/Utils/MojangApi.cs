using System.Globalization;
using System.Net.Http.Json;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Utils
{
    public class MojangApiResponse
    {
        public string id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public Property[] properties { get; set; } = Array.Empty<Property>();
    }
    public class Property
    {
        public string name { get; set; } = string.Empty;
        public string value { get; set; } = string.Empty;
        public string? signature { get; set; }
    }

    public static class MojangApi
    {

        private const string SessionEndpoint = "https://sessionserver.mojang.com/session/minecraft/hasJoined";

        public static async Task<MojangApiResponse?> HasJoined(string username, byte[] aesCipher, byte[] publicKey, string? ipAddress = null)
        {

            using HttpClient client = new();

            string serverId = MinecraftShaDigest(aesCipher.Concat(publicKey));

            string escapedUsername = Uri.EscapeDataString(username);
            string escapedServerId = Uri.EscapeDataString(serverId);
            string uri = SessionEndpoint + $"?username={escapedUsername}&serverId={escapedServerId}";

            if (null != ipAddress)
                uri += $"&ip={Uri.EscapeDataString(ipAddress)}";

            HttpResponseMessage response = await client.GetAsync(uri);

            if( response.StatusCode != System.Net.HttpStatusCode.OK )
                return null;

            return await response.Content.ReadFromJsonAsync<MojangApiResponse>();
        }

        // https://gist.github.com/ammaraskar/7b4a3f73bee9dc4136539644a0f27e63
        internal static string MinecraftShaDigest(IEnumerable<byte> data)
        {
            var hash = SHA1.HashData(data.ToArray());
            // Reverse the bytes since BigInteger uses little endian
            Array.Reverse(hash);

            var b = new System.Numerics.BigInteger(hash);
            // very annoyingly, BigInteger in C# tries to be smart and puts in
            // a leading 0 when formatting as a hex number to allow roundtripping
            // of negative numbers, thus we have to trim it off.
            if (b < 0)
            {
                // toss in a negative sign if the interpreted number is negative
                return $"-{(-b).ToString("x", CultureInfo.InvariantCulture).TrimStart('0')}";
            }
            else
            {
                return b.ToString("x", CultureInfo.InvariantCulture).TrimStart('0');
            }
        }
    }
}
