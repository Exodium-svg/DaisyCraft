using System;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nbt.Serialization.JsonConverters
{
    public class ColorJsonConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Case 1: Null or empty → return Color.Empty
            if (reader.TokenType == JsonTokenType.Null)
                return Color.Empty;

            // Case 2: Integer → treat as RGB
            if (reader.TokenType == JsonTokenType.Number)
            {
                int value = reader.GetInt32();
                byte r = (byte)((value >> 16) & 0xFF);
                byte g = (byte)((value >> 8) & 0xFF);
                byte b = (byte)(value & 0xFF);
                return Color.FromArgb(r, g, b);
            }

            // Case 3: String → hex or named color
            if (reader.TokenType == JsonTokenType.String)
            {
                string? s = reader.GetString();
                if (string.IsNullOrWhiteSpace(s))
                    return Color.Empty;

                // Try named colors ("red", "blue", etc.)
                var named = Color.FromName(s);
                if (named.IsKnownColor || named.IsNamedColor)
                    return named;

                // Strip '#' if present
                if (s.StartsWith("#"))
                    s = s[1..];

                if (s.Length == 6)
                {
                    int r = Convert.ToInt32(s.Substring(0, 2), 16);
                    int g = Convert.ToInt32(s.Substring(2, 2), 16);
                    int b = Convert.ToInt32(s.Substring(4, 2), 16);
                    return Color.FromArgb(r, g, b);
                }
                else if (s.Length == 8)
                {
                    int a = Convert.ToInt32(s.Substring(0, 2), 16);
                    int r = Convert.ToInt32(s.Substring(2, 2), 16);
                    int g = Convert.ToInt32(s.Substring(4, 2), 16);
                    int b = Convert.ToInt32(s.Substring(6, 2), 16);
                    return Color.FromArgb(a, r, g, b);
                }

                throw new JsonException($"Invalid color string: {s}");
            }

            throw new JsonException($"Unexpected token parsing Color: {reader.TokenType}");
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            if (value.IsEmpty)
            {
                writer.WriteNullValue();
                return;
            }

            // For consistency, output hex string with alpha if not 255
            string hex = value.A < 255
                ? $"#{value.A:X2}{value.R:X2}{value.G:X2}{value.B:X2}"
                : $"#{value.R:X2}{value.G:X2}{value.B:X2}";

            writer.WriteStringValue(hex);
        }
    }

}
