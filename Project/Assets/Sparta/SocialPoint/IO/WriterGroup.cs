
namespace SocialPoint.IO
{
    public class WriterGroup : IWriter
    {
        IWriter[] _writers;

        public WriterGroup(IWriter[] writers)
        {
            _writers = writers;
        }

        public void Write(ushort value)
        {
            for (int i = 0; i < _writers.Length; ++i)
            {
                _writers[i].Write(value);
            }
        }

        public void Write(sbyte value)
        {
            for (int i = 0; i < _writers.Length; ++i)
            {
                _writers[i].Write(value);
            }
        }

        public void Write(char value)
        {
            for (int i = 0; i < _writers.Length; ++i)
            {
                _writers[i].Write(value);
            }
        }

        public void Write(int value)
        {
            for (int i = 0; i < _writers.Length; ++i)
            {
                _writers[i].Write(value);
            }
        }

        public void Write(double value)
        {
            for (int i = 0; i < _writers.Length; ++i)
            {
                _writers[i].Write(value);
            }
        }

        public void Write(ulong value)
        {
            for (int i = 0; i < _writers.Length; ++i)
            {
                _writers[i].Write(value);
            }
        }

        public void Write(uint value)
        {
            for (int i = 0; i < _writers.Length; ++i)
            {
                _writers[i].Write(value);
            }
        }

        public void Write(long value)
        {
            for (int i = 0; i < _writers.Length; ++i)
            {
                _writers[i].Write(value);
            }
        }

        public void Write(float value)
        {
            for (int i = 0; i < _writers.Length; ++i)
            {
                _writers[i].Write(value);
            }
        }

        public void Write(string value)
        {
            for (int i = 0; i < _writers.Length; ++i)
            {
                _writers[i].Write(value);
            }
        }

        public void Write(byte value)
        {
            for (int i = 0; i < _writers.Length; ++i)
            {
                _writers[i].Write(value);
            }
        }

        public void Write(short value)
        {
            for (int i = 0; i < _writers.Length; ++i)
            {
                _writers[i].Write(value);
            }
        }

        public void Write(bool value)
        {
            for (int i = 0; i < _writers.Length; ++i)
            {
                _writers[i].Write(value);
            }
        }

        public void Write(byte[] buffer, int count)
        {
            for (int i = 0; i < _writers.Length; ++i)
            {
                _writers[i].Write(buffer, count);
            }
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            for (int i = 0; i < _writers.Length; ++i)
            {
                _writers[i].Write(buffer, count);
            }
        }

        public void WriteShortFloat(float value)
        {
            for (int i = 0; i < _writers.Length; ++i)
            {
                _writers[i].WriteShortFloat(value);
            }
        }
    }
}
