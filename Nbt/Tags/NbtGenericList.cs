using System.Collections;

namespace Nbt.Tags
{
    public struct NbtGenericList : IEnumerable, INbtTag
    {
        public TagType ElementType { get; init; }
        public TagType Type { get; init; } = TagType.List;
        public string Key { get; init; }

        public object? Value => throw new NotImplementedException();
        private List<INbtTag> tags;

        public NbtGenericList(string key, TagType elementType, int capacity = 0) {

            Key = key;
            ElementType = elementType;
            if(capacity > 0)
                tags = new List<INbtTag>(capacity);
            else
                tags = new List<INbtTag>();
        }

        IEnumerator IEnumerable.GetEnumerator() => tags.GetEnumerator();

        public void Add(INbtTag item) => tags.Add(item);

        public object this[int index] => tags[index].Value!;
        
        
    }
}
