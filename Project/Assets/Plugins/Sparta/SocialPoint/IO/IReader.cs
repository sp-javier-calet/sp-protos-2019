
namespace SocialPoint.IO
{
    public interface IReader
    {
        bool ReadBoolean();

        byte ReadByte();

        byte[] ReadBytes(int count);

        double ReadDouble();

        short ReadInt16();

        int ReadInt32();

        long ReadInt64();

        float ReadSingle();

        string ReadString();

        ushort ReadUInt16();

        uint ReadUInt32();

        ulong ReadUInt64();

        float ReadShortFloat();

        bool Finished{ get; }
    }

    public abstract class SimpleReader : IReader
    {
        public abstract bool ReadBoolean();

        public abstract byte ReadByte();

        public abstract double ReadDouble();

        public abstract short ReadInt16();

        public abstract int ReadInt32();

        public abstract long ReadInt64();

        public abstract float ReadSingle();

        public abstract string ReadString();

        public abstract ushort ReadUInt16();

        public abstract uint ReadUInt32();

        public abstract ulong ReadUInt64();

        public abstract bool Finished{ get; }

        public byte[] ReadBytes(int count)
        {
            var bytes = new byte[count];
            for(var i = 0; i < count; i++)
            {
                bytes[i] = ReadByte();
            }
            return bytes;
        }

        public float ReadShortFloat()
        {
            return ShortEncoding.Decode(ReadInt16());
        }
    }
}
