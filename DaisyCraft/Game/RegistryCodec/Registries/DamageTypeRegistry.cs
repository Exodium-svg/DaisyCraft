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
            writer.WriteValue(new NbtString("message_id", MessageId));
            writer.WriteValue(new NbtString("scaling", Scaling));
            writer.WriteValue(new NbtFloat("exhaustion", Exhaustion));

            if(null != Effects)
                writer.WriteValue(new NbtString("effects", Effects));

            if (null != DeathMessageType)
                writer.WriteValue(new NbtString("death_message_type", DeathMessageType));
        }
    }
}
