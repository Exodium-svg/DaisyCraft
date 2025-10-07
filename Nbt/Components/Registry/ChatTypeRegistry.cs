using Nbt.Registries;
using System.Text.Json.Serialization;

namespace Nbt.Components.Registry
{
    [RegistryType("chat_type")]
    public partial struct ChatTypeRegistry : IRegistryEntry, INbtComponent
    {
        [JsonPropertyName("chat")]
        [NbtComponentType("chat", ComponentType.NbtComponent)]
        public DecorationComponent Chat { get; set; }
        [JsonPropertyName("narration")]
        [NbtComponentType("narration", ComponentType.NbtComponent)]
        public DecorationComponent Narration { get; set; }

    }
}
