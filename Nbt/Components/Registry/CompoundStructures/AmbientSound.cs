using System.Text.Json.Serialization;

namespace Nbt.Components
{
    public partial struct AmbientSound : INbtComponent
    {
        [JsonPropertyName("sound_id")]
        [NbtComponentTypeAttribute("sound_id", ComponentType.String)]
        public string SoundId { get; set; }
        [JsonPropertyName("range")]
        [NbtComponentTypeAttribute("range", ComponentType.Float)]
        public float? Range { get; set; }
    }
}
