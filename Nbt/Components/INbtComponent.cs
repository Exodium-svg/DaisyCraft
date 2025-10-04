using Nbt.Serialization;
using Nbt.Tags;

namespace Nbt.Components
{
    public enum ComponentType
    {
        String,
        HexColor,
        IntColor,
        Bool,
        Byte,
        Short,
        Int,
        Long,
        Float,
        Double,
        NbtArray,
        LongArray,
        IntArray,
    }
    public class NbtComponentType : Attribute
    {
        public string Key { get; init; }
        public ComponentType Type { get; init; }

        public NbtComponentType(string key, ComponentType type)
        {
            Key = key;
            Type = type;
        }
    }
    public interface INbtComponent
    {
        public void Write(NbtWriter writer);
        public void Read(ref readonly NbtCompound compoundTag);
    }
}
