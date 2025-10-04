namespace Nbt.Tags
{
    public struct NbtEndTag : INbtTag
    {
        public TagType Type { get; init; } = TagType.End;
        public string Key { get; init; } = string.Empty;

        public NbtEndTag() { }
    }
}
