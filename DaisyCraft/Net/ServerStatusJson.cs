using System.Text.Json.Serialization;

namespace Net
{
    public class ServerStatus
    {
        [JsonPropertyName("version")]
        public VersionInfo Version { get; set; } = null!;

        [JsonPropertyName("enforcesSecureChat")]
        public bool EnforcesSecureChat { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("players")]
        public PlayersInfo Players { get; set; } = null!;

        [JsonPropertyName("favicon")]
        public string? Icon { get; set; } = "data:image/png;base64,";
        public ServerStatus(int version, bool enforcesSecureChat, string description, int maxPlayers, int onlinePlayers)
        {
            Version = new VersionInfo { Name = "1.21.9", Protocol = version };
            EnforcesSecureChat = enforcesSecureChat;
            Description = description;
            Players = new PlayersInfo { Max = maxPlayers, Online = onlinePlayers };
        }
    }

    public class VersionInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("protocol")]
        public int Protocol { get; set; }
    }

    public class PlayersInfo
    {
        [JsonPropertyName("max")]
        public int Max { get; set; }

        [JsonPropertyName("online")]
        public int Online { get; set; }
    }
}
