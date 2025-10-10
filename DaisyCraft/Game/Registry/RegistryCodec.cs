using DaisyCraft.Game.Registry;
using Nbt.Components;
using Nbt.Tags;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace Game.Registry
{
    public class RegistryCodec
    {
        public IReadOnlyDictionary<string, RegistryObject> Entries { get; init; }

        public ConcurrentDictionary<string, IRegistry> RegistryEntries { get; init; } = new();

        public RegistryCodec(Dictionary<string, RegistryObject> registries) => Entries = new ReadOnlyDictionary<string, RegistryObject>(registries);
        
        public Registry<T> Get<T>(string tag) where T : INbtComponent
        {
            Identifier id = new Identifier { Name = tag, Namespace = "minecraft" };
            string registryId = id.ToString();
            if (RegistryEntries.TryGetValue(registryId, out var registryEntry))
                return ((Registry<T>)registryEntry);

            T? value = Activator.CreateInstance<T>();
            
            if (null == value)
                throw new Exception($"Invalid value, no default constructor found for: {typeof(T).Name}");

            if (!Entries.TryGetValue($"minecraft:{tag}", out var registry))
                throw new KeyNotFoundException($"No registry found for tag '{tag}'");

            NbtCompound compoundTag = registry.Root;
            value.Read(ref compoundTag);

            Registry<T> newRegistry = new Registry<T>(id, value);
            RegistryEntries[registryId] = newRegistry;

            return newRegistry;
        }

        public List<NbtCompound> GetAll(string nameSpace) => Entries.Values.Where((RegistryObject registry) => registry.Identifier.Namespace == nameSpace).Select( (register) => register.Root ).ToList();
        public RegistryObject Get(string tag)
        {
            if (!Entries.TryGetValue($"minecraft:{tag}", out var registry))
                throw new KeyNotFoundException($"No registry found for tag '{tag}'");

            return registry;
        }
    }
}
