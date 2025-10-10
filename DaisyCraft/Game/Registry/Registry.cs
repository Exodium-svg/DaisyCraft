using Nbt.Components;

namespace DaisyCraft.Game.Registry
{
    public interface  IRegistry
    {
        public string Name { get; init; }
        public string Namespace { get; init; }
    }
    public class Registry<T> : IRegistry where T : INbtComponent
    {
        public string Name { get; init; }
        public string Namespace { get; init; }
        public T Value { get; init; }

        public Registry(string name, string nameSpace, T value)
        {
            Name = name;
            Namespace = nameSpace;
            Value = value;
        }
    }
}
