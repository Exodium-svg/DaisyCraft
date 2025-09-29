using Nbt.Tags;

namespace Nbt.Serialization
{
    public class NetNbtBuilder : INbtBuilder
    {

        public INbtTag? RootTag { get; init; } = null;
        public Dictionary<string, INbtTag> Tags { get; set; }

        public NetNbtBuilder(Dictionary<string, INbtTag> tags) => Tags = tags;
        

        public NetNbtBuilder() => Tags = new();
        
        public void Add<T>(T obj)
        {
            throw new NotImplementedException();
        }

        public void AddTag(INbtTag tag) => Tags.Add(tag.Key, tag);

        public void RemoveTag(INbtTag tag) => Tags.Remove(tag.Key);
        public Span<byte> Build()
        {
            // everything is written as raw java type binary. ( besides the enum which is a byte ) honestly, I will just only use NetNBT as the otherone is not useful.
            // we will not use a stream here, as we simply just do not need it
            //int offset = 0;
            List<byte> buffer = new();
            NbtWriteHelper.WriteTag(buffer, TagType.Compound);

            NbtWriteHelper.WriteTags(buffer, Tags.Values);

            NbtWriteHelper.WriteTag(buffer, TagType.End);

            return new Span<byte>(buffer.ToArray());
        }

        public void Build(Stream stream) => stream.Write(Build());


    }
}
