using System.IO;

namespace SocialPoint.IO
{
    public sealed class SystemBinaryReader : IReader
    {
        readonly BinaryReader _reader;

        public SystemBinaryReader(BinaryReader reader)
        {
            _reader = reader;
        }

        public SystemBinaryReader(Stream stream):
        this(new BinaryReader(stream))
        {
        }

        public bool ReadBoolean()
        {
            return _reader.ReadBoolean();
        }

        public byte ReadByte()
        {
            return _reader.ReadByte();
        }

        public byte[] ReadBytes(int count)
        {
            return _reader.ReadBytes(count);
        }

        public double ReadDouble()
        {
            return _reader.ReadDouble();
        }

        public short ReadInt16()
        {
            return _reader.ReadInt16();
        }

        public int ReadInt32()
        {
            return _reader.ReadInt32();
        }

        public long ReadInt64()
        {
            return _reader.ReadInt64();
        }

        public float ReadSingle()
        {
            return _reader.ReadSingle();
        }

        public string ReadString()
        {
            return _reader.ReadString();
        }

        public ushort ReadUInt16()
        {
            return _reader.ReadUInt16();
        }

        public uint ReadUInt32()
        {
            return _reader.ReadUInt32();
        }

        public ulong ReadUInt64()
        {
            return _reader.ReadUInt64();
        }

        public bool EOF()
        {
            return _reader.PeekChar() == -1;
        }

        public float ReadShortFloat()
        {
            return ShortEncoding.Decode(_reader.ReadInt16());
        }

        public void Dispose()
        {
            _reader.Close();
        }

        public bool Finished
        {
            get
            {
                return _reader.BaseStream.Position >= _reader.BaseStream.Length;
            }
        }
    }
}
