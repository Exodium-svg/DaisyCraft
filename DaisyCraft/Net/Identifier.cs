using System.Text;

namespace Net
{
    public struct Identifier
    {
        public string Tag { get; init; }
        public byte[] Value { get; init; }

        public Identifier(string tag, byte[] value)
        {
            Tag = tag;
            Value = value;
        }

        public string GetString() => Encoding.UTF8.GetString(Value);
        public T GetValue<T>() where T : struct => throw new Exception("Implement me lazy piece of shit");
    }
}
