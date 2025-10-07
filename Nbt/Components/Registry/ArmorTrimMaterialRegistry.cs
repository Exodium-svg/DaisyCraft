using Nbt.Registries;
using Nbt.Tags;
using System.Text.Json.Serialization;

namespace Nbt.Components.Registry
{
    [RegistryType("trim_material")]
    public partial struct ArmorTrimMaterialRegistry : IRegistryEntry, INbtComponent
    {
        [JsonPropertyName("asset_name")]
        [NbtComponentTypeAttribute("asset_name", ComponentType.String)]
        public string AssetName { get; set; }
        [JsonPropertyName("ingredient")]
        [NbtComponentTypeAttribute("ingredient", ComponentType.String)]
        public string Ingredient { get; set; }
        [JsonPropertyName("item_model_index")]
        [NbtComponentTypeAttribute("item_model_index", ComponentType.Float)]
        public float ItemModelIndex { get; set; }

        [JsonPropertyName("override_armor_materials")]
        [NbtComponentTypeAttribute("override_armor_materials", ComponentType.Compound)]
        public NbtCompound? OverrideArmorMaterials { get; set; }

        [JsonPropertyName("description")]
        [NbtComponentTypeAttribute("description", ComponentType.NbtComponent)]
        public TextComponent Description { get; set; }
    }
}
