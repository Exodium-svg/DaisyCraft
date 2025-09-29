using Nbt.Tags;

namespace Nbt.Serialization
{
    public interface INbtBuilder
    {
        public INbtTag? RootTag { get; init; }
        //public Dictionary<string, INbtTag> Tags { get; set; }

        public void AddTag(INbtTag tag);
        public void RemoveTag(INbtTag tag);

        public Span<byte> Build();
        public void Build(Stream stream);
        public void Add<T>(T obj);
    }
}
