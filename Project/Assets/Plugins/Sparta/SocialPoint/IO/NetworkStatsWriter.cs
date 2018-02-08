namespace SocialPoint.IO
{
    public class NetworkStatsWriter : IWriter
    {
        readonly IWriter _writer;

        int _dataLength;

        public int DataLength
        {
            get
            {
                return _dataLength;
            }
        }

        public NetworkStatsWriter(IWriter writer)
        {
            _writer = writer;
            _dataLength = 0;
        }

        #region IWriter implementation

        void IWriter.Write(bool value)
        {
            _dataLength += sizeof(bool);
            _writer.Write(value);
        }

        void IWriter.Write(byte[] buffer, int offset, int count)
        {
            _dataLength += buffer.Length;
            _writer.Write(buffer, offset, count);
        }

        void IWriter.Write(ushort value)
        {
            _dataLength += sizeof(ushort);
            _writer.Write(value);
        }

        void IWriter.Write(short value)
        {
            _dataLength += sizeof(short);
            _writer.Write(value);
        }

        void IWriter.Write(sbyte value)
        {
            _dataLength += sizeof(sbyte);
            _writer.Write(value);
        }

        void IWriter.Write(byte value)
        {
            _dataLength += sizeof(byte);
            _writer.Write(value);
        }

        void IWriter.Write(char value)
        {
            _dataLength += sizeof(char);
            _writer.Write(value);
        }

        void IWriter.Write(int value)
        {
            _dataLength += sizeof(int);
            _writer.Write(value);
        }

        void IWriter.Write(byte[] buffer, int count)
        {
            _dataLength += buffer.Length;
            _writer.Write(buffer, count);
        }

        void IWriter.Write(string value)
        {
            System.Text.Encoding.Unicode.GetByteCount(value);
            _writer.Write(value);
        }

        void IWriter.Write(double value)
        {
            _dataLength += sizeof(double);
            _writer.Write(value);
        }

        void IWriter.Write(float value)
        {
            _dataLength += sizeof(float);
            _writer.Write(value);
        }

        void IWriter.Write(ulong value)
        {
            _dataLength += sizeof(ulong);
            _writer.Write(value);
        }

        void IWriter.Write(long value)
        {
            _dataLength += sizeof(long);
            _writer.Write(value);
        }

        void IWriter.Write(uint value)
        {
            _dataLength += sizeof(uint);
            _writer.Write(value);
        }

        void IWriter.WriteShortFloat(float value)
        {
            _dataLength += sizeof(float);
            _writer.Write(value);
        }

        #endregion
    }
}

