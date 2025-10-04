namespace Nbt.Tags
{
    public struct NbtIntArray : INbtTag
    {
        public TagType Type { get; init; } = TagType.IntArray;
        public string Key { get; init; }
        public int[] Value { get; set; }
        public NbtIntArray(string key, int[] value)
        {
            Key = key;
            Value = value;
        }

        public static implicit operator int[](NbtIntArray b) => b.Value;
    }
}
