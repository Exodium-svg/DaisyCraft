using Nbt.Registries;
using Nbt.Tags;
using System.Text.Json.Serialization;

namespace Nbt.Components.Registry
{
    [RegistryType("trim_pattern")]
    public partial struct ArmorTrimPatternRegistry : IRegistryEntry, INbtComponent
    {
        [JsonPropertyName("asset_id")]
        [NbtComponentType("asset_id", ComponentType.String)]
        public string AssetId { get; set; }
        [JsonPropertyName("template_item")]
        [NbtComponentType("template_item", ComponentType.String)]
        public string TemplateItem { get; set; }
        [JsonPropertyName("description")]
        [NbtComponentType("description", ComponentType.NbtComponent)]
        public TextComponent Description { get; set; }
        [JsonPropertyName("decal")]
        [NbtComponentType("decal", ComponentType.Byte)]
        public byte Decal { get; set; }
    }
}
