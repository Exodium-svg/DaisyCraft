namespace Nbt.Tags
{
    public struct NbtFloat : INbtTag
    {
        public TagType Type { get; init; } = TagType.Float;
        public string Key { get; init; }
        public float Value { get; set; }
        public NbtFloat(string key, float value)
        {
            Key = key;
            Value = value;
        }

        public static implicit operator float(NbtFloat b) => b.Value;
    }
}
