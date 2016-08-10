using SocialPoint.IO;
using System;

namespace SocialPoint.Multiplayer
{    
    public class DirtyBits
    {
        const int MaxSize = 256;

        bool[] _data = new bool[MaxSize];
        int _size;
        int _position;

        public int Count
        {
            get
            {
                return _size;
            }
        }

        public bool Finished
        {
            get
            {
                return _position >= _size;
            }
        }

        public bool Get()
        {
            if(Finished)
            {
                throw new InvalidOperationException("Cannot read over the size.");
            }
            var v = _data[_position];
            _position++;
            return v;
        }

        public void Set(bool v)
        {
            if(_size >= MaxSize)
            {
                throw new InvalidOperationException("Max size reached.");
            }
            _data[_position] = v;
            _position++;
            if(_position > _size)
            {
                _size = _position;
            }
        }

        public void Clear()
        {
            _size = 0;
            _position = 0;
        }

        public void Reset()
        {
            _position = 0;
        }

        public void Read(IReader reader, int size)
        {
            if(size > MaxSize)
            {
                throw new InvalidOperationException("Cannot read more than max size.");
            }
            for(var i = 0; i < size; i++)
            {
                _data[i] = reader.ReadBoolean();
            }
            _size = size;
        }

        public void Write(IWriter writer, int size=-1)
        {
            if(size < 0 || size > _size)
            {
                size = _size;
            }
            for(var i = 0; i < size; i++)
            {
                writer.Write(_data[i]);
            }
        }
    }
}
