namespace Game.RegistryCodec.Registries
{
    public class RegistryInfo : Attribute
    {
        public string Id { get; init; }
        public RegistryInfo(string id) => Id = id;
    }
    public interface IRegistryEntry
    {
        
    }
}
