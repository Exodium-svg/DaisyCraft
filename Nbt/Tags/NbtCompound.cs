namespace Nbt.Tags
{
    public struct NbtCompound : INbtTag
    {
        public TagType Type { get; init; } = TagType.Compound;
        public string Key { get; init; }
        public Dictionary<string, INbtTag> Tags { get; init; } = new();

        public object? Value => Tags;

        public NbtCompound(string key) => Key = key;
    }
}
