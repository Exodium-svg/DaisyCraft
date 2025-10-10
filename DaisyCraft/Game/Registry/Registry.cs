using Nbt.Components;

namespace DaisyCraft.Game.Registry
{
    public interface  IRegistry
    {
        public Identifier Identifier { get; init; }
    }
    public class Registry<T> : IRegistry where T : INbtComponent
    {
        public Identifier Identifier { get; init; }
        public T Value { get; init; }

        public Registry(string name, string nameSpace, T value)
        {
            Identifier = new Identifier { Name = name, Namespace = nameSpace };
            Value = value;
        }
        public Registry(Identifier id, T value)
        {
            Identifier = id;
            Value = value;
        }
    }
}
