namespace Nbt.Tags
{
    public struct NbtCompound : INbtTag
    {
        public TagType Type { get; init; } = TagType.Compound;
        public string Key { get; init; }

        public readonly List<INbtTag> tags;

        public NbtCompound(string key)
        {
            Key = key;
            tags = new List<INbtTag>();
        }

        public NbtCompound()
        {
            Key = string.Empty;
            tags = new();
        }

        public bool ContainsKey(string key)
        {
            foreach (var tag in tags)
                if (tag.Key == key)
                    return true;
            return false;
        }

        public bool TryGetValue(string key, out INbtTag? tag)
        {
            foreach (var candidate in tags)
                if (candidate.Key == key)
                {
                    tag = candidate;
                    return true;
                }

            tag = null;
            return false;
        }

        public INbtTag this[string key]
        {
            get
            {
                foreach (var tag in tags)
                    if (tag.Key == key)
                        return tag;

                throw new KeyNotFoundException($"Tag with key '{key}' not found in compound '{Key}'.");
            }
            set
            {
                for (int i = 0; i < tags.Count; i++)
                {
                    if (tags[i].Key == key)
                    {
                        tags[i] = value;
                        return;
                    }
                }

                tags.Add(value);
            }
        }
    }
}
