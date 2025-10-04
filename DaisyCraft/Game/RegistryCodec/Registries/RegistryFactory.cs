using System.Text.Json;

namespace Game.RegistryCodec.Registries
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
                default:
                    return null;
            }
        }
    }
}
