
namespace Nbt.Tags
{
    public struct NbtDouble : INbtTag
    {
        public TagType Type { get; init; } = TagType.Double;
        public string Key { get; init; }
        public double Value { get; set; }
        public NbtDouble(string key, double value)
        {
            Key = key;
            Value = value;
        }

        public static implicit operator double(NbtDouble b) => b.Value;
    }
}
