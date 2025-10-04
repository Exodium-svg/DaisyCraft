namespace Nbt.Tags
{
    public struct NbtString : INbtTag
    {
        public TagType Type { get; init; } = TagType.String;
        public string Key { get; init; }
        public string Value { get; set; }

        public NbtString(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public static implicit operator string(NbtString b) => b.Value;
    }
}
