namespace Nbt.Tags
{
    public struct NbtLongArray : INbtTag
    {
        public TagType Type { get; init; } = TagType.LongArray;
        public string Key { get; init; }
        public long[] Value { get; set; }
        public NbtLongArray(string key, long[] value)
        {
            Key = key;
            Value = value;
        }

        public static implicit operator long[](NbtLongArray b) => b.Value;
    }
}
