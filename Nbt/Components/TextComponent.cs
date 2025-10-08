using Nbt.Serialization.JsonConverters;
using System.Drawing;
using System.Text.Json.Serialization;

namespace Nbt.Components
{
    public partial struct TextComponent : INbtComponent
    {
        [JsonConverter(typeof(ColorJsonConverter))]
        [JsonPropertyName("color")]
        [NbtComponentTypeAttribute("color", ComponentType.HexColor)]
        public Color? TextColor { get; set; }
        [JsonConverter(typeof(ColorJsonConverter))]
        [JsonPropertyName("shadow_color")]
        [NbtComponentTypeAttribute("shadow_color", ComponentType.IntColor)]
        public Color? ShadowColor { get; set; }
        [JsonPropertyName("bold")]
        [NbtComponentTypeAttribute("bold", ComponentType.Bool)]
        public bool? Bold { get; set; }
        [JsonPropertyName("italic")]
        [NbtComponentTypeAttribute("italic", ComponentType.Bool)]
        public bool? Italic { get; set; }
        [JsonPropertyName("underlined")]
        [NbtComponentTypeAttribute("underlined", ComponentType.Bool)]
        public bool? Underlined { get; set; }
        [JsonPropertyName("strikethrough")]
        [NbtComponentTypeAttribute("strikethrough", ComponentType.Bool)]
        public bool? StrikeThrough { get; set; }
        [JsonPropertyName("obfuscated")]
        [NbtComponentTypeAttribute("obfuscated", ComponentType.Bool)]
        public bool? Obfuscated { get; set; }
        [JsonPropertyName("type")]
        [NbtComponentTypeAttribute("type", ComponentType.String)]
        public string? Type { get; set; }
        [JsonPropertyName("text")]
        [NbtComponentTypeAttribute("text", ComponentType.String)]
        public string Text { get; set; }
        [JsonPropertyName("translate")]
        [NbtComponentTypeAttribute("translate", ComponentType.String)]
        public string? TranslationKey { get; set; }
        [JsonPropertyName("fallback")]
        [NbtComponentTypeAttribute("fallback", ComponentType.String)]
        public string? Fallback { get; set; }

        public static TextComponent CreateText(string text) => new TextComponent()
        {
            Type = "text",
            Text = text,
        };

        public static TextComponent CreateTranslation(string key, string fallback) => new TextComponent()
        {
            Type = "translatable",
            TranslationKey = key,
            Fallback = fallback
        };
    }
}
