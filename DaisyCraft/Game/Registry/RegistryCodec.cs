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
            if(RegistryEntries.TryGetValue(tag, out var registryEntry))
                return ((Registry<T>)registryEntry);

            T? value = Activator.CreateInstance<T>();
            
            if (null == value)
                throw new Exception($"Invalid value, no default constructor found for: {typeof(T).Name}");

            if (!Entries.TryGetValue(tag, out var registry))
                throw new KeyNotFoundException($"No registry found for tag '{tag}'");

            NbtCompound compoundTag = registry.Root;
            value.Read(ref compoundTag);

            Registry<T> newRegistry = new Registry<T>(registry.Name, registry.Namespace, value);
            RegistryEntries[$"{registry.Namespace}:{registry.Name}"] = newRegistry;

            return newRegistry;
        }
    }
}
