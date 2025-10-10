namespace DaisyCraft.Game.Registry
{
    public struct Identifier
    {
        public string Name { get; init; }
        public string Namespace { get; init; }

        public override string ToString() => $"{Namespace}:{Name}";

        public Identifier(string name, string nameSpace)
        {
            Name = name;
            Namespace = nameSpace;
        }
    }
}
