using DaisyCraft;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace Utils
{
    public static class MojangApi
    {
        const string baseUrl = "https://sessionserver.mojang.com/";

        public static async Task<bool> HasJoined(string username, string serverId, byte[] aesCipher, byte[] publicKey)
        {
            string id = MinecraftShaDigest(Encoding.UTF8.GetBytes(serverId).Concat(aesCipher).Concat(publicKey).ToArray());
            


            using HttpClient client = new();

            string uri = baseUrl + $"minecraft/hasJoined?username={username}&serverId={id}";

            HttpResponseMessage response = await client.GetAsync(uri);
            Console.WriteLine(await response.Content.ReadAsStringAsync());

            return true;
        }
        private static string MinecraftShaDigest(byte[] input)
        {
            byte[] hash;
            using (SHA1 sha1 = SHA1.Create())
            {
                hash = sha1.ComputeHash(input);
            }

            // Reverse the bytes since BigInteger expects little-endian
            Array.Reverse(hash);

            BigInteger b = new BigInteger(hash);

            // Format according to Minecraft's session server expectation
            if (b < 0)
            {
                // Negative number: prefix with "-"
                return "-" + (-b).ToString("x").TrimStart('0');
            }
            else
            {
                return b.ToString("x").TrimStart('0');
            }
        }
    }
}
