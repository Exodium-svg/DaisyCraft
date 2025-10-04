using Nbt.Tags;
using System.Drawing;
using System.Text.Json.Serialization;

namespace Nbt.Components
{
    public partial struct TextComponent : INbtComponent
    {

        [JsonPropertyName("color")]
        [NbtComponentType("color" ,ComponentType.HexColor)]
        public Color? TextColor { get; set; }
        [JsonPropertyName("shadow_color")]
        [NbtComponentType("shadow_color", ComponentType.IntColor)]
        public Color? ShadowColor { get; set; }
        [JsonPropertyName("bold")]
        [NbtComponentType("bold", ComponentType.Bool)]
        public bool? Bold { get; set; }
        [JsonPropertyName("italic")]
        [NbtComponentType("italic", ComponentType.Bool)]
        public bool? Italic { get; set; }
        [JsonPropertyName("underlined")]
        [NbtComponentType("underlined", ComponentType.Bool)]
        public bool? Underlined { get; set; }
        [JsonPropertyName("strikethrough")]
        [NbtComponentType("strikethrough", ComponentType.Bool)]
        public bool? StrikeThrough { get; set; }
        [JsonPropertyName("obfuscated")]
        [NbtComponentType("obfuscated", ComponentType.Bool)]
        public bool? Obfuscated { get; set; }
        [JsonPropertyName("type")]
        [NbtComponentType("type", ComponentType.String)]
        private string? Type { get; set; }
        [JsonPropertyName("text")]
        [NbtComponentType("text", ComponentType.String)]
        private string Text { get; set; }
        [JsonPropertyName("translate")]
        [NbtComponentType("translate", ComponentType.String)]
        private string? TranslationKey { get; set; }
        [JsonPropertyName("fallback")]
        [NbtComponentType("fallback", ComponentType.String)]
        private string? Fallback { get; set; }
        public void SetText(string text)
        {
            Type = "text";
            Text = text;
        }

        public void SetTranslation(string key, string fallback = "UNKNOWN_KEY")
        {
            Type = "translatable";
            TranslationKey = key;
            Fallback = fallback;
        }
    }
}
