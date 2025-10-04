namespace Nbt.Tags
{
    public struct NbtByte : INbtTag
    {
        public TagType Type { get; init; } = TagType.Byte;
        public string Key { get; init; }
        public byte Value { get; set; }
        public NbtByte(string key, byte value)
        {
            Key = key;
            Value = value;
        }

        public static implicit operator byte(NbtByte b) => b.Value;
    }
}
