using Nbt.Components;
using Nbt.Tags;
using System.Text.Json.Serialization;

namespace Game.RegistryCodec.Registries
{
    [RegistryInfo("damage_type")]
    public class DamageTypeRegistry : IRegistryEntry, INbtComponent
    {
        [JsonPropertyName("message_id")]
        public string MessageId { get; set; }
        [JsonPropertyName("scaling")]
        public string Scaling { get; set; }
        [JsonPropertyName("exhaustion")]
        public float Exhaustion { get; set; }
        [JsonPropertyName("effects")]
        public string? Effects { get; set; }
        [JsonPropertyName("death_message_type")]
        public string? DeathMessageType { get; set; }

        public void Read(ref readonly NbtCompound compoundTag)
        {
            

        }

        public void Write(NbtWriter writer)
        {
            writer.WriteTag(new NbtString("message_id", MessageId));
            writer.WriteTag(new NbtString("scaling", Scaling));
            writer.WriteTag(new NbtFloat("exhaustion", Exhaustion));

            if(null != Effects)
                writer.WriteTag(new NbtString("effects", Effects));

            if (null != DeathMessageType)
                writer.WriteTag(new NbtString("death_message_type", DeathMessageType));
        }
    }
}
