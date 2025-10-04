using Nbt.Tags;
using System.Buffers.Binary;
using System.Text;

public class NbtWriter : IDisposable
{
    private readonly Stream _stream;
    private readonly bool isNetwork;
    public NbtWriter(Stream stream, bool isNetwork = true)
    {
        this.isNetwork = isNetwork;
        _stream = stream;

        if(isNetwork)
            WriteType(TagType.NetCompound);


    }

    private void WriteType(TagType type) => _stream.WriteByte((byte)type);
    private void WriteKey(string key) => WriteString(key);
    private void WriteString(string str)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(str);
        WriteShort((short)bytes.Length);
        _stream.Write(bytes, 0, bytes.Length);
    }

    private void WriteShort(short value)
    {
        Span<byte> buffer = stackalloc byte[2];
        BinaryPrimitives.WriteInt16BigEndian(buffer, value);
        _stream.Write(buffer);
    }

    private void WriteInt(int value)
    {
        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(buffer, value);
        _stream.Write(buffer);
    }

    private void WriteLong(long value)
    {
        Span<byte> buffer = stackalloc byte[8];
        BinaryPrimitives.WriteInt64BigEndian(buffer, value);
        _stream.Write(buffer);
    }

    private void WriteFloat(float value)
    {
        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteSingleBigEndian(buffer, value);
        _stream.Write(buffer);
    }

    private void WriteDouble(double value)
    {
        Span<byte> buffer = stackalloc byte[8];
        BinaryPrimitives.WriteDoubleBigEndian(buffer, value);
        _stream.Write(buffer);
    }

    private void WriteByte(byte value) => _stream.WriteByte(value);
    
    private void WriteByteArray(byte[] array)
    {
        WriteInt(array.Length);
        _stream.Write(array, 0, array.Length);
    }

    private void WriteIntArray(int[] array)
    {
        WriteInt(array.Length);

        foreach (int value in array)
            WriteInt(value);
    }

    private void WriteLongArray(long[] array)
    {
        WriteInt(array.Length);

        foreach (long value in array)
            WriteLong(value);
    }

    public void WriteTag(INbtTag tag)
    {
        WriteType(tag.Type);
        WriteKey(tag.Key);
        WriteValue(tag);
    }

    public void WriteValue(INbtTag tag)
    {
        switch (tag.Type)
        {
            case TagType.Byte:
                WriteByte((NbtByte)tag);
                break;
            case TagType.Short:
                WriteShort((NbtShort)tag);
                break;
            case TagType.Int:
                WriteInt((NbtInt)tag);
                break;
            case TagType.Long:
                WriteLong((NbtLong)tag);
                break;
            case TagType.Float:
                WriteFloat((NbtFloat)tag);
                break;
            case TagType.Double:
                WriteDouble((NbtDouble)tag);
                break;
            case TagType.ByteArray:
                WriteByteArray((NbtByteArray)tag);
                break;
            case TagType.String:
                WriteString((NbtString)tag);
                break;
            case TagType.List:
                WriteList((IEnumerable<INbtTag>)tag);
                break;
            case TagType.Compound:
                WriteCompound((IEnumerable<INbtTag>)tag);
                break;
            case TagType.IntArray:
                WriteIntArray((NbtIntArray)tag);
                break;
            case TagType.LongArray:
                WriteLongArray((NbtLongArray)tag);
                break;
            default:
                throw new ArgumentException($"Unsupported tag type: {tag.Type}");
        }
    }

    private void WriteList(IEnumerable<INbtTag> list)
    {
        int count = list.Count();
        TagType elementType = count > 0 ? list.First().Type : TagType.End;
        WriteType(elementType);
        WriteInt(count);

        foreach (var element in list)
            WriteValue(element);
    }

    private void WriteCompound(IEnumerable<INbtTag> compound)
    {
        foreach (var tag in compound)
            WriteTag(tag);
        
        WriteType(TagType.End);
    }

    public void Dispose()
    {
        if (isNetwork)
            WriteType(TagType.End);
    }
}
