using System;
using SocialPoint.IO;

namespace SocialPoint.WebSockets
{
    public class WebSocketsTextReader : IReader
    {
        string _content;

        public WebSocketsTextReader(string content)
        {
            _content = content;
        }

        #region IReader implementation

        public bool Finished{ get; private set; }

        public bool ReadBoolean()
        {
            throw new NotImplementedException();
        }

        public byte ReadByte()
        {
            throw new NotImplementedException();
        }

        public byte[] ReadBytes(int count)
        {
            throw new NotImplementedException();
        }

        public double ReadDouble()
        {
            throw new NotImplementedException();
        }

        public short ReadInt16()
        {
            throw new NotImplementedException();
        }

        public int ReadInt32()
        {
            throw new NotImplementedException();
        }

        public long ReadInt64()
        {
            throw new NotImplementedException();
        }

        public float ReadSingle()
        {
            throw new NotImplementedException();
        }

        public string ReadString()
        {
            Finished = true;
            return _content;
        }

        public ushort ReadUInt16()
        {
            throw new NotImplementedException();
        }

        public uint ReadUInt32()
        {
            throw new NotImplementedException();
        }

        public ulong ReadUInt64()
        {
            throw new NotImplementedException();
        }

        public float ReadShortFloat()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
