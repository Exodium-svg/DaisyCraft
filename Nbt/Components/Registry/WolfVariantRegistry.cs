using Nbt.Components;
using System.Text.Json.Serialization;

namespace Nbt.Registries
{
    [RegistryType("wolf_variant")]
    public partial struct WolfVariantRegistry : IRegistryEntry, INbtComponent
    {
        [JsonPropertyName("assets")]
        [NbtComponentTypeAttribute("assets", ComponentType.NbtComponent)]
        public WolfAssets Assets { get; set; }

        [JsonPropertyName("spawn_conditions")]
        [NbtComponentTypeAttribute("spawn_conditions", ComponentType.NbtArray)]
        public SpawnConditionEntry[] SpawnConditions { get; set; }
    }

    public partial struct WolfAssets : INbtComponent
    {
        [JsonPropertyName("wild")]
        [NbtComponentTypeAttribute("wild", ComponentType.String)]
        public string Wild { get; set; }

        [JsonPropertyName("tame")]
        [NbtComponentTypeAttribute("tame", ComponentType.String)]
        public string Tame { get; set; }

        [JsonPropertyName("angry")]
        [NbtComponentTypeAttribute("angry", ComponentType.String)]
        public string Angry { get; set; }
    }

    public partial struct SpawnConditionEntry : INbtComponent
    {
        [JsonPropertyName("condition")]
        [NbtComponentTypeAttribute("condition", ComponentType.NbtComponent)]
        public SpawnCondition Condition { get; set; }

        [JsonPropertyName("priority")]
        [NbtComponentTypeAttribute("priority", ComponentType.Int)]
        public int Priority { get; set; }
    }

    public partial struct SpawnCondition : INbtComponent
    {
        [JsonPropertyName("type")]
        [NbtComponentTypeAttribute("type", ComponentType.String)]
        public string Type { get; set; }

        [JsonPropertyName("biomes")]
        [NbtComponentTypeAttribute("biomes", ComponentType.String)]
        public string Biomes { get; set; }
    }
}


