namespace Nbt.Tags
{
    public struct NbtTag<T> : INbtTag
    {
        public TagType Type { get; init; }
        public string Key { get; init; }

        public object? Value { get; set; }

        public NbtTag(string key, T? value) {
            Type = typeof(T) switch
            {
                var t when t == typeof(byte) => TagType.Byte,
                var t when t == typeof(short) => TagType.Short,
                var t when t == typeof(int) => TagType.Int,
                var t when t == typeof(long) => TagType.Long,
                var t when t == typeof(float) => TagType.Float,
                var t when t == typeof(double) => TagType.Double,
                var t when t == typeof(string) => TagType.String,
                
                _ => throw new NotSupportedException($"Unsupported type {typeof(T)} for NBT tag.")
            };

            Key = key;
            Value = value;
        }
    }
}
