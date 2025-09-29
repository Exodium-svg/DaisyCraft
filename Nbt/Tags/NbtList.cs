using System.Collections;
using System.Numerics;

namespace Nbt.Tags
{
    public struct NbtList<T> : INbtTag, IEnumerable 
        where T : INumber<T>
    {
        public TagType Type { get; init; }
        public string Key { get; init; }

        public object? Value { get => (object)buffer; }

        private List<T> buffer;

        public NbtList(string key, int capacity = 0) {

            Type = typeof(T) switch
            {
                var t when t == typeof(int) => TagType.IntArray,
                var t when t == typeof(long) => TagType.LongArray,
                _ => throw new NotSupportedException($"Unsupported type {typeof(T)} for NBTList tag.")
            };

            Key = key;

            if (capacity > 0)
                buffer = new List<T>(capacity);
            else
                buffer = new List<T>();
        }

        public IEnumerator GetEnumerator() => ((IEnumerable)buffer).GetEnumerator();

        public T this[int index]
        {
            get => buffer[index];
            set => buffer[index] = value;
        }
    }
}
