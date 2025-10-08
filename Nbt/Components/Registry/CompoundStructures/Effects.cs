using Nbt.Components.Registry;
using Nbt.Tags;
using System.Text.Json.Serialization;

namespace Nbt.Components
{

    public partial struct Effects : INbtComponent
    {
        [JsonPropertyName("fog_color")]
        [NbtComponentTypeAttribute("fog_color", ComponentType.Int)]
        public int FogColor { get; set; }
        [JsonPropertyName("water_color")]
        [NbtComponentTypeAttribute("water_color", ComponentType.Int)]
        public int WaterColor { get; set; }
        [JsonPropertyName("water_fog_color")]
        [NbtComponentTypeAttribute("water_fog_color", ComponentType.Int)]
        public int WaterFogColor { get; set; }
        [JsonPropertyName("sky_color")]
        [NbtComponentTypeAttribute("sky_color", ComponentType.Int)]
        public int SkyColor { get; set; }
        [JsonPropertyName("foliage_color")]
        [NbtComponentTypeAttribute("foliage_color", ComponentType.Int)]
        public int? FoliageColor { get; set; }
        [JsonPropertyName("grass_color")]
        [NbtComponentTypeAttribute("grass_color", ComponentType.Int)]
        public int? GrassColor { get; set; }
        [JsonPropertyName("grass_color_modifier")]
        [NbtComponentTypeAttribute("grass_color_modifier", ComponentType.String)]
        public string? GrassColorModifier { get; set; }
        [JsonPropertyName("particle")]
        [NbtComponentTypeAttribute("particle", ComponentType.Compound)]
        public NbtCompound? Particle { get; set; }
        [JsonPropertyName("ambient_sound")]
        [NbtComponentTypeAttribute("ambient_sound", ComponentType.NbtComponent)]
        public AmbientSound? AmbientSound { get; set; }
        [JsonPropertyName("mood_sound")]
        [NbtComponentTypeAttribute("mood_sound", ComponentType.NbtComponent)]
        public MoodSound? Moodsound { get; set; }
        [JsonPropertyName("additions_sound")]
        [NbtComponentTypeAttribute("additions_sound", ComponentType.NbtComponent)]
        public AdditionsSound? AdditionsSound { get; set; }
        [JsonPropertyName("music")]
        [NbtComponentTypeAttribute("music", ComponentType.NbtComponent)]
        public Music? Music { get; set; }
    }
}
