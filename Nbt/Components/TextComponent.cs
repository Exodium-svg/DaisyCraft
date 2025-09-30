using Nbt.Tags;
using System.Drawing;

namespace Nbt.Components
{
    public struct TextComponent
    {
        Dictionary<string, INbtTag> nbtTags = new();

        public TextComponent() { }

        public void SetColor(Color color) => nbtTags["color"] = new NbtTag<string>("color", $"#{color.R:X2}{color.G:X2}{color.B:X2}");
        public void SetShadow(Color color) => nbtTags["shadow_color"] = new NbtTag<int>("shadow_color", (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B);
        public void SetBold(bool isBold) => nbtTags["bold"] = new NbtTag<byte>("bold", isBold ? (byte)1 : (byte)0);
        public void SetItalic(bool isItalic) => nbtTags["italic"] = new NbtTag<byte>("italic", isItalic ? (byte)1: (byte)0);
        public void SetUnderlined(bool isUnderLined) => nbtTags["underlined"] = new NbtTag<byte>("underlined", isUnderLined ? (byte)1: (byte)0);
        public void SetStrikeThrough(bool isStrikeThrough) => nbtTags["strikethrough"] = new NbtTag<byte>("strikethrough", isStrikeThrough ? (byte)1: (byte)0);
        public void SetObfuscated(bool isObfuscated) => nbtTags["obfuscated"] = new NbtTag<byte>("obfuscated", isObfuscated ? (byte)1: (byte)0);

        public void SetText(string text)
        {
            nbtTags["type"] = new NbtTag<string>("type", "text");
            nbtTags["text"] = new NbtTag<string>("text", text);
        }

        public void SetTranslation(string key, string fallback = "UNKNOWN_KEY")
        {
            nbtTags["type"] = new NbtTag<string>("type", "translatable");
            nbtTags["translate"] = new NbtTag<string>("translate", key);
            nbtTags["fallback"] = new NbtTag<string>("fallback", fallback);
        }
        
    }
}
