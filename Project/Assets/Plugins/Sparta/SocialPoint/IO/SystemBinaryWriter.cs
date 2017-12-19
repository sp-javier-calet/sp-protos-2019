using System.IO;

namespace SocialPoint.IO
{
    public sealed class SystemBinaryWriter : IWriter
    {
        readonly BinaryWriter _writer;

        public SystemBinaryWriter(BinaryWriter writer)
        {
            _writer = writer;
        }

        public SystemBinaryWriter(Stream stream):
        this(new BinaryWriter(stream))
        {
        }

        public void Write(bool value)
        {
            _writer.Write(value);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            _writer.Write(buffer, offset, count);
        }

        public void Write(ushort value)
        {
            _writer.Write(value);
        }

        public void Write(short value)
        {
            _writer.Write(value);
        }

        public void Write(sbyte value)
        {
            _writer.Write(value);
        }

        public void Write(byte value)
        {
            _writer.Write(value);
        }

        public void Write(char value)
        {
            _writer.Write(value);
        }

        public void Write(int value)
        {
            _writer.Write(value);
        }

        public void Write(byte[] buffer, int count)
        {
            _writer.Write(buffer, 0, count);
        }

        public void Write(string value)
        {
            _writer.Write(value);
        }

        public void Write(double value)
        {
            _writer.Write(value);
        }

        public void Write(float value)
        {
            _writer.Write(value);
        }

        public void Write(ulong value)
        {
            _writer.Write(value);
        }

        public void Write(long value)
        {
            _writer.Write(value);
        }

        public void Write(uint value)
        {
            _writer.Write(value);
        }

        public void WriteShortFloat(float value)
        {
            _writer.Write(ShortEncoding.Encode(value));
        }

        public void Flush()
        {
            _writer.Flush();
        }
    }
}