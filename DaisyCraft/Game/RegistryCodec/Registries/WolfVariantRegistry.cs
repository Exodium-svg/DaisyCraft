using System.Text.Json.Serialization;

namespace Game.RegistryCodec.Registries
{
    [RegistryInfo("wolf_variant")]
    public class WolfVariantRegistry : IRegistryEntry
    {
        [JsonPropertyName("assets")]
        public WolfAssets Assets { get; set; }

        [JsonPropertyName("spawn_conditions")]
        public List<SpawnConditionEntry> SpawnConditions { get; set; }
    }

    public class WolfAssets
    {
        [JsonPropertyName("wild")]
        public string Wild { get; set; }

        [JsonPropertyName("tame")]
        public string Tame { get; set; }

        [JsonPropertyName("angry")]
        public string Angry { get; set; }
    }

    public class SpawnConditionEntry
    {
        [JsonPropertyName("condition")]
        public SpawnCondition Condition { get; set; }

        [JsonPropertyName("priority")]
        public int Priority { get; set; }
    }

    public class SpawnCondition
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("biomes")]
        public string Biomes { get; set; }
    }
}
