using Nbt.Registries;
using Nbt.Tags;
using System.Text.Json.Serialization;

namespace Nbt.Components.Registry
{
    [RegistryType("worldgen/biome")]
    public partial struct BiomeRegistry : IRegistryEntry, INbtComponent
    {
        [JsonPropertyName("has_precipitation")]
        [NbtComponentTypeAttribute("has_precipitation", ComponentType.Byte)]
        public byte HasPrecipitation { get; set; }

        [JsonPropertyName("temperature")]
        [NbtComponentTypeAttribute("temperature", ComponentType.Float)]
        public float Temperature { get; set; }
        [JsonPropertyName("temperature_modifier")]
        [NbtComponentTypeAttribute("temperature_modifier", ComponentType.String)]
        public string? TemperatureModifier { get; set; }
        [JsonPropertyName("downfall")]
        [NbtComponentTypeAttribute("downfall", ComponentType.Float)]
        public float DownFall { get; set; }
        [JsonPropertyName("effects")]
        [NbtComponentTypeAttribute("effects", ComponentType.NbtComponent)]
        public Effects Effects { get; set; }

    }


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

    public partial struct AmbientSound : INbtComponent
    {
        [JsonPropertyName("sound_id")]
        [NbtComponentTypeAttribute("sound_id", ComponentType.String)]
        public string SoundId { get; set; }
        [JsonPropertyName("range")]
        [NbtComponentTypeAttribute("range", ComponentType.Float)]
        public float? Range { get; set; }
    }
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

    public partial struct AdditionsSound : INbtComponent
    {
        [JsonPropertyName("sound")]
        [NbtComponentTypeAttribute("sound", ComponentType.String)]
        public string Sound { get; set; }
        [JsonPropertyName("probability")]
        [NbtComponentTypeAttribute("probability", ComponentType.Double)]
        public double TickChance { get; set; }
    }

    public partial struct Music : INbtComponent     
    {
        [JsonPropertyName("sound")]
        [NbtComponentTypeAttribute("sound", ComponentType.String)]
        public string Sound { get; set; }
        [JsonPropertyName("min_delay")]
        [NbtComponentTypeAttribute("min_delay", ComponentType.Int)]
        public int MinDelay { get; set; }
        [JsonPropertyName("max_delay")]
        [NbtComponentTypeAttribute("max_delay", ComponentType.Int)]
        public int MaxDelay { get; set; }
        [JsonPropertyName("replace_current_music")]
        [NbtComponentTypeAttribute("replace_current_music", ComponentType.Byte)]
        public byte ReplaceCurrentMusic { get; set; }
    }
}
