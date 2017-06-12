using System;
using SocialPoint.IO;

namespace SocialPoint.WebSockets
{   
    public class WebSocketsTextWriter : IWriter
    {
        string _content;

        #region IWriter implementation

        public void Write(bool value)
        {
            throw new NotImplementedException();
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public void Write(ushort value)
        {
            throw new NotImplementedException();
        }

        public void Write(short value)
        {
            throw new NotImplementedException();
        }

        public void Write(sbyte value)
        {
            throw new NotImplementedException();
        }

        public void Write(byte value)
        {
            throw new NotImplementedException();
        }

        public void Write(char value)
        {
            throw new NotImplementedException();
        }

        public void Write(int value)
        {
            throw new NotImplementedException();
        }

        public void Write(byte[] buffer, int count)
        {
            throw new NotImplementedException();
        }

        public void Write(string value)
        {
            if(_content != null)
            {
                throw new InvalidOperationException("Writer content is already set");
            }

            _content = value;
        }

        public void Write(double value)
        {
            throw new NotImplementedException();
        }

        public void Write(float value)
        {
            throw new NotImplementedException();
        }

        public void Write(ulong value)
        {
            throw new NotImplementedException();
        }

        public void Write(long value)
        {
            throw new NotImplementedException();
        }

        public void Write(uint value)
        {
            throw new NotImplementedException();
        }

        public void WriteShortFloat(float value)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override string ToString()
        {
            return _content;
        }
    }
}
