namespace Nbt.Tags
{
    public struct NbtByteArray : INbtTag
    {
        public TagType Type { get; init; } = TagType.ByteArray;
        public string Key { get; init; }
        public byte[] Value { get; set; }
        public NbtByteArray(string key, byte[] value)
        {
            Key = key;
            Value = value;
        }

        public static implicit operator byte[](NbtByteArray b) => b.Value;
    }
}
