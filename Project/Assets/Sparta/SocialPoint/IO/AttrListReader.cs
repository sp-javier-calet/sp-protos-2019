using System.IO;
using SocialPoint.Attributes;

namespace SocialPoint.IO
{
    public class AttrReader : SimpleReader
    {
        readonly AttrList _attr;
        int _position;

        public AttrReader(AttrList attr)
        {
            _attr = attr;
            _position = 0;
        }

        public override bool Finished
        {
            get
            {
                return _position >= _attr.Count;
            }
        }

        AttrValue GetValue()
        {
            var v = _attr.GetValue(_position);
            _position++;
            return v;
        }

        public override bool ReadBoolean()
        {
            return GetValue().ToBool();
        }

        public override byte ReadByte()
        {
            return (byte)ReadUInt32();
        }

        public override double ReadDouble()
        {
            return GetValue().ToDouble();
        }

        public override short ReadInt16()
        {
            return (short)ReadUInt32();
        }

        public override int ReadInt32()
        {
            return GetValue().ToInt();
        }

        public override long ReadInt64()
        {
            return GetValue().ToInt();
        }

        public override float ReadSingle()
        {
            return GetValue().ToFloat();
        }

        public override string ReadString()
        {
            return GetValue().ToString();
        }

        public override ushort ReadUInt16()
        {
            return (ushort)ReadInt32();
        }

        public override uint ReadUInt32()
        {
            return (uint)ReadInt32();
        }

        public override ulong ReadUInt64()
        {
            return (ulong)ReadInt64();
        }
    }
}
