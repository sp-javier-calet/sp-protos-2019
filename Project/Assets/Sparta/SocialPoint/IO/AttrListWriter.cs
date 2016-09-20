using System.IO;
using SocialPoint.Attributes;

namespace SocialPoint.IO
{
    public class AttrWriter : SimpleWriter
    {
        readonly AttrList _attr;

        public AttrWriter(AttrList attr)
        {
            _attr = attr;
        }

        public override void Write(bool value)
        {
            _attr.AddValue(value);
        }

        public override void Write(ushort value)
        {
            _attr.AddValue(value);
        }

        public override void Write(short value)
        {
            _attr.AddValue(value);
        }

        public override void Write(sbyte value)
        {
            _attr.AddValue(value);
        }

        public override void Write(byte value)
        {
            _attr.AddValue(value);
        }

        public override void Write(char value)
        {
            _attr.AddValue(value);
        }

        public override void Write(int value)
        {
            _attr.AddValue(value);
        }

        public override void Write(string value)
        {
            _attr.AddValue(value);
        }

        public override void Write(double value)
        {
            _attr.AddValue(value);
        }

        public override void Write(float value)
        {
            _attr.AddValue(value);
        }

        public override void Write(ulong value)
        {
            _attr.AddValue(value);
        }

        public override void Write(long value)
        {
            _attr.AddValue(value);
        }

        public override void Write(uint value)
        {
            _attr.AddValue(value);
        }
    }
}