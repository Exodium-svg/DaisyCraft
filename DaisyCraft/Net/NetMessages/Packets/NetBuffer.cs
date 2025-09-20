using System.Buffers;
using System.Diagnostics;

namespace Net.NetMessages.Packets
{
    public class NetBuffer : IDisposable
    {
        public Player Owner {get;set;}
        public int Length { get {
                if (null == Buffer)
                    return 0;

                return Buffer.Length;
            } }

        public byte[]? Buffer { get; private set; } // can be null if disposed
        public bool Compressed { get; set; } = false;

        private ArrayPool<byte> pool;

        public NetBuffer(Player owner, ArrayPool<byte> pool)
        {
            Owner = owner;
            Buffer = null;
            this.pool = pool;
        }

        public MemoryStream GetStream()
        {
            if (null == Buffer)
                throw new Exception("Invalid state, does not own a Buffer");

            return new MemoryStream(Buffer);
        }

        public void Reserve(int size)
        {
            Debug.Assert(size > 0);

            if( null != Buffer )
                pool.Return(Buffer);
            
            Buffer = pool.Rent(size);
        }

        public void Dispose()
        {
            if (null != Buffer)
            {
                pool.Return(Buffer);
                Buffer = null;
            }

        }
    }
}
