namespace Nbt.Tags
{
    public struct NbtInt : INbtTag
    {
        public TagType Type { get; init; } = TagType.Int;
        public string Key { get; init; }
        public int Value { get; set; }
        public NbtInt(string key, int value)
        {
            Key = key;
            Value = value;
        }

        public static implicit operator int(NbtInt b) => b.Value;
    }
}
