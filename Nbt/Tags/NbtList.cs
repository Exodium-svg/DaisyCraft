namespace Nbt.Tags
{
    public struct NbtList : INbtTag
    {
        public TagType Type { get; init; } = TagType.List;
        public TagType ElementType { get; init; }
        public string Key { get; init; }
        public INbtTag[] Value { get; set; }
        public NbtList(string key, TagType elementType, INbtTag[] value)
        {
            ElementType = elementType;
            Key = key;
            Value = value;
        }

        public static implicit operator INbtTag[](NbtList b) => b.Value;
    }
}
