using System.IO;
using System;
using SocialPoint.Attributes;

namespace SocialPoint.IO
{
    public class AttrWriter : IWriter
    {
        readonly AttrList _attr;

        public AttrWriter(AttrList attr)
        {
            _attr = attr;
        }

        public void Write(bool value)
        {
            _attr.AddValue(value);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if(buffer.Length - offset < count)
            {
                throw new ArgumentException("buffer too short");
            }
            for(var i = 0; i < count; i++)
            {
                Write(buffer[i + offset]);
            }
        }

        public void Write(ushort value)
        {
            _attr.AddValue(value);
        }

        public void Write(short value)
        {
            _attr.AddValue(value);
        }

        public void Write(sbyte value)
        {
            _attr.AddValue(value);
        }

        public void Write(byte value)
        {
            _attr.AddValue(value);
        }

        public void Write(char value)
        {
            _attr.AddValue(value);
        }

        public void Write(int value)
        {
            _attr.AddValue(value);
        }

        public void Write(byte[] buffer, int count)
        {
            Write(buffer, 0, count);
        }

        public void Write(string value)
        {
            _attr.AddValue(value);
        }

        public void Write(double value)
        {
            _attr.AddValue(value);
        }

        public void Write(float value)
        {
            _attr.AddValue(value);
        }

        public void Write(ulong value)
        {
            _attr.AddValue(value);
        }

        public void Write(long value)
        {
            _attr.AddValue(value);
        }

        public void Write(uint value)
        {
            _attr.AddValue(value);
        }
    }
}