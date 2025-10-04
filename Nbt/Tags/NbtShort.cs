namespace Nbt.Tags
{
    public struct NbtShort : INbtTag
    {
        public TagType Type { get; init; } = TagType.Short;
        public string Key { get; init; }
        public short Value { get; set; }
        public NbtShort(string key, short value)
        {
            Key = key;
            Value = value;
        }

        public static implicit operator short(NbtShort b) => b.Value;
    }
}
