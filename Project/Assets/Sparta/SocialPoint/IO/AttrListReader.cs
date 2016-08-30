using System.IO;
using SocialPoint.Attributes;

namespace SocialPoint.IO
{
    public class AttrReader : IReader
    {
        readonly AttrList _attr;
        int _position;

        public AttrReader(AttrList attr)
        {
            _attr = attr;
            _position = 0;
        }

        AttrValue GetValue()
        {
            var v = _attr.GetValue(_position);
            _position++;
            return v;
        }

        public bool ReadBoolean()
        {
            return GetValue().ToBool();
        }

        public byte ReadByte()
        {
            return (byte)ReadUInt32();
        }

        public byte[] ReadBytes(int count)
        {
            var bytes = new byte[count];
            for(var i = 0; i < count; i++)
            {
                bytes[i] = ReadByte();
            }
            return bytes;
        }

        public double ReadDouble()
        {
            return GetValue().ToDouble();
        }

        public short ReadInt16()
        {
            return (short)ReadUInt32();
        }

        public int ReadInt32()
        {
            return GetValue().ToInt();
        }

        public long ReadInt64()
        {
            return GetValue().ToInt();
        }

        public float ReadSingle()
        {
            return GetValue().ToFloat();
        }

        public string ReadString()
        {
            return GetValue().ToString();
        }

        public ushort ReadUInt16()
        {
            return (ushort)ReadInt32();
        }

        public uint ReadUInt32()
        {
            return (uint)ReadInt32();
        }

        public ulong ReadUInt64()
        {
            return (ulong)ReadInt64();
        }
    }
}