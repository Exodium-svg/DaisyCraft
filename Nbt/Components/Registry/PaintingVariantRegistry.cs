using Nbt.Registries;
using System.Text.Json.Serialization;

namespace Nbt.Components.Registry
{
    [RegistryType("painting_variant")]
    public partial struct PaintingVariantRegistry : IRegistryEntry, INbtComponent
    {
        [JsonPropertyName("asset_id")]
        [NbtComponentType("asset_id", ComponentType.String)]
        public string AssetId { get; set; }
        [JsonPropertyName("height")]
        [NbtComponentType("height", ComponentType.Int)]
        public int Height { get; set; }
        [JsonPropertyName("width")]
        [NbtComponentType("width", ComponentType.Int)]
        public int Width { get; set; }
        [JsonPropertyName("title")]
        [NbtComponentType("title", ComponentType.NbtComponent)]
        public TextComponent Title { get; set; }
        [JsonPropertyName("author")]
        [NbtComponentType("author", ComponentType.NbtComponent)]
        public TextComponent Author { get; set; }
    }
}
