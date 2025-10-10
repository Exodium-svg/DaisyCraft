using Nbt.Tags;
using System.Text.Json;

namespace DaisyCraft.Game.Registry
{
    public class RegistryObject : IRegistry
    {
        public NbtCompound Root { get; set; }
        public Identifier Identifier { get; init; }

        public RegistryObject(string name, string nameSpace, NbtCompound root)
        {
            Identifier = new Identifier { Name = name, Namespace = nameSpace };
            Root = root;
        }
        public RegistryObject(Identifier id, NbtCompound root)
        {
            Identifier = id;
            Root = root;
        }
    }
}
