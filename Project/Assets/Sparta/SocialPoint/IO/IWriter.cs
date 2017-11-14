using System;

namespace SocialPoint.IO
{
    public interface IWriter
    {
        void Write(bool value);

        void Write(byte[] buffer, int offset, int count);

        void Write(ushort value);

        void Write(short value);

        void Write(sbyte value);

        void Write(byte value);

        void Write(char value);

        void Write(int value);

        void Write(byte[] buffer, int count);

        void Write(string value);

        void Write(double value);

        void Write(float value);

        void Write(ulong value);

        void Write(long value);

        void Write(uint value);

        void WriteShortFloat(float value);
    }

    public abstract class SimpleWriter : IWriter
    {
        public abstract void Write(bool value);

        public abstract void Write(ushort value);

        public abstract void Write(short value);

        public abstract void Write(sbyte value);

        public abstract void Write(byte value);

        public abstract void Write(char value);

        public abstract void Write(int value);       

        public abstract void Write(string value);

        public abstract void Write(double value);

        public abstract void Write(float value);

        public abstract void Write(ulong value);

        public abstract void Write(long value);

        public abstract void Write(uint value);

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

        public void Write(byte[] buffer, int count)
        {
            Write(buffer, 0, count);
        }

        public void WriteShortFloat(float value)
        {
            Write(ShortEncoding.Encode(value));
        }
    }
}