using Nbt.Components.Registry;
using System.Text.Json;

namespace Nbt.Registries
{
    public static class RegistryFactory
    {
        public static IRegistryEntry? Create(string nameSpace, string json)
        {
            switch (nameSpace)
            {
                case "damage_type":
                    return JsonSerializer.Deserialize<DamageTypeRegistry>(json);
                case "wolf_variant":
                    return JsonSerializer.Deserialize<WolfVariantRegistry>(json);
                case "chat_type":
                    return JsonSerializer.Deserialize<ChatTypeRegistry>(json);
                case "painting_variant":
                    return JsonSerializer.Deserialize<PaintingVariantRegistry>(json);
                case "trim_material":
                    return JsonSerializer.Deserialize<ArmorTrimMaterialRegistry>(json);
                case "trim_pattern":
                    return JsonSerializer.Deserialize<ArmorTrimPatternRegistry>(json);
                case "worldgen/biome":
                    return JsonSerializer.Deserialize<BiomeRegistry>(json);
                case "banner_pattern":
                    return JsonSerializer.Deserialize<BannerPatternRegistry>(json);
                default:
                    return null;
            }
        }
    }
}
