using Nbt.Components;
using System.Text.Json.Serialization;

namespace Nbt.Registries
{
    [RegistryType("damage_type")]
    public partial struct DamageTypeRegistry : IRegistryEntry, INbtComponent
    {
        [JsonPropertyName("message_id")]
        [NbtComponentTypeAttribute("message_id", ComponentType.String)]
        public string MessageId { get; set; }
        [JsonPropertyName("scaling")]
        [NbtComponentTypeAttribute("scaling", ComponentType.String)]
        public string Scaling { get; set; }
        [JsonPropertyName("exhaustion")]
        [NbtComponentTypeAttribute("exhaustion", ComponentType.Float)]
        public float Exhaustion { get; set; }
        [JsonPropertyName("effects")]
        [NbtComponentTypeAttribute("effects", ComponentType.String)]
        public string? Effects { get; set; }
        [JsonPropertyName("death_message_type")]
        [NbtComponentTypeAttribute("death_message_type", ComponentType.String)]
        public string? DeathMessageType { get; set; }
    }
}
