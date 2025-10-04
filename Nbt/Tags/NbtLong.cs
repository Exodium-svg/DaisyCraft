namespace Nbt.Tags
{
    public struct NbtLong : INbtTag
    {
        public TagType Type { get; init; } = TagType.Long;
        public string Key { get; init; }
        public long Value { get; set; }
        public NbtLong(string key, long value)
        {
            Key = key;
            Value = value;
        }

        public static implicit operator long(NbtLong b) => b.Value;
    }
}
