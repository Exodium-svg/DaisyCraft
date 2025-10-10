using Nbt.Tags;
using System.Text.Json;

namespace DaisyCraft.Game.Registry
{
    public class RegistryObject
    {
        public string Name { get; init; }
        public string Namespace { get; init; }
        public NbtCompound Root { get; set; }
        public RegistryObject(string name, string ns, NbtCompound root)
        {
            Name = name;
            Namespace = ns;
            Root = root;
        }
    }
}
