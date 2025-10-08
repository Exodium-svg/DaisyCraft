using System.Text.Json.Serialization;

namespace Nbt.Components
{
    public partial struct MoodSound : INbtComponent
    {
        [JsonPropertyName("sound")]
        [NbtComponentTypeAttribute("sound", ComponentType.String)]
        public string Sound { get; set; }
        [JsonPropertyName("tick_delay")]
        [NbtComponentTypeAttribute("tick_delay", ComponentType.Int)]
        public int TickDelay { get; set; }
        [JsonPropertyName("block_search_extent")]
        [NbtComponentTypeAttribute("block_search_extent", ComponentType.Int)]
        public int BlockSearchExtent { get; set; }

        [JsonPropertyName("offset")]
        [NbtComponentTypeAttribute("offset", ComponentType.Double)]
        public double Offset { get; set; }
    }
}
