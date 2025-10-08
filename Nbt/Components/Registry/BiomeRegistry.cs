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
}
