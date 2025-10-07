namespace Nbt.Registries
{
    public class RegistryType : Attribute
    {
        public string Id { get; init; }
        public RegistryType(string id) => Id = id;
    }
    public interface IRegistryEntry
    {
        
    }
}
