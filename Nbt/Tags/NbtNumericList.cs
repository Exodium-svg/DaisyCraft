using System.Collections;
using System.Numerics;

namespace Nbt.Tags
{
    public struct NbtNumericList<T> : INbtTag, IEnumerable, IEnumerable<T>
        where T : INumber<T>
    {
        public TagType Type { get; init; }
        public string Key { get; init; }

        public object? Value { get => (object)buffer; }

        private List<T> buffer;

        public NbtNumericList(string key, int capacity = 0) {

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

        public void AddRange(IEnumerable<T> values) => buffer.AddRange(values);
        public IEnumerator GetEnumerator() => ((IEnumerable)buffer).GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => buffer.GetEnumerator();
        
        public T this[int index]
        {
            get => buffer[index];
            set => buffer[index] = value;
        }
    }
}
