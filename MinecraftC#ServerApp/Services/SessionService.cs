using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Utils;

namespace Services
{
    public class SessionService
    {
        public string Dir { get; init; }

        public SessionService(string directory)
        {
            if(!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            Dir = directory;
        }


        public async Task<MojangApiResponse?> GetUser(Guid uuid, IPEndPoint remoteEndpoint, string username, byte[] cipherKey, byte[] certificate)
        {
            string identifier = uuid.ToString() + remoteEndpoint.Address.ToString().Split(':')[0];

            string path = Path.Combine(Dir, $"{identifier}.session");

            MojangApiResponse? response;
            if (!File.Exists(path))
            {
                response = await MojangApi.HasJoined(username, cipherKey, certificate);

                if (null == response)
                    return null;

                using FileStream fs = File.Create(path);

                JsonSerializer.Serialize<MojangApiResponse>(fs, response);
            }
            else
            {

                using FileStream fs = File.OpenRead(path);

                response = JsonSerializer.Deserialize<MojangApiResponse>(fs);
            }


            return response;
        }
    }
}
