using Nbt.Registries;
using System.Text.Json.Serialization;

namespace Nbt.Components.Registry
{
    [RegistryType("banner_pattern")]
    public partial struct BannerPatternRegistry : IRegistryEntry, INbtComponent
    {
        [JsonPropertyName("asset_id")]
        [NbtComponentType("asset_id", ComponentType.String)]
        public string AssetId { get; set; }
        [JsonPropertyName("translation_key")]
        [NbtComponentType("translation_key", ComponentType.String)]
        public string TranslationKey { get; set; }
    }
}
