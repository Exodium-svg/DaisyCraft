using Nbt.Tags;
using System.Text.Json.Serialization;

namespace Nbt.Components
{
    public partial struct DecorationComponent : INbtComponent
    {
        [JsonPropertyName("translation_key")]
        [NbtComponentTypeAttribute("translation_key", ComponentType.String)]
        public string TranslationKey { get; set; }
        [JsonPropertyName("style")]
        [NbtComponentTypeAttribute("style", ComponentType.Compound)]
        public NbtCompound? Style { get; set; }
        [JsonPropertyName("parameters")]
        [NbtComponentTypeAttribute("parameters", ComponentType.NbtArray)]
        public string[] Parameters { get; set; }
    }
}
