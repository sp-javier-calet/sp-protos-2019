using UnityEngine.Networking;
using SocialPoint.IO;

namespace SocialPoint.Network
{
    public sealed class UnetNetworkWriter : IWriter
    {
        readonly NetworkWriter _writer;

        public UnetNetworkWriter(NetworkWriter writer)
        {
            _writer = writer;
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
            _writer.Write(buffer, count);
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
    }
}