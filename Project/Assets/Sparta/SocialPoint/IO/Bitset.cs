using System;

namespace SocialPoint.IO
{    
    public class Bitset
    {
        const int MaxSize = 64;

        UInt64 _data;
        int _position;

        int _size;

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
            var v = (_data & (1UL << _position)) != 0UL;
            _position++;
            return v;
        }

        public void Set(bool v)
        {
            if(_size >= MaxSize)
            {
                throw new InvalidOperationException("Max size reached.");
            }
            if(v)
            {
                _data |= (1UL << _position);
            }
            else
            {
                _data &= ~(1UL << _position);
            }
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
                throw new InvalidOperationException("Cannot read more than the max size.");
            }
            if(size <= 0)
            {
                _data = 0UL;
            }
            else if(size <= 8)
            {
                _data = (UInt64)reader.ReadByte();
            }
            else if(size <= 16)
            {
                _data = (UInt64)reader.ReadUInt16();
            }
            else if(size <= 32)
            {
                _data = (UInt64)reader.ReadInt32();
            }
            else
            {
                _data = reader.ReadUInt64();
            }
            _size = size;
        }

        public void Write(IWriter writer, int size=-1)
        {
            if(size > _size)
            {
                throw new InvalidOperationException("Cannot read more than the size.");
            }
            if(size < 0)
            {
                size = _size;
            }
            if(size == 0)
            {
                // nothing to write
            }
            else if(size <= 8)
            {
                writer.Write((byte)_data);
            }
            else if(size <= 16)
            {
                writer.Write((UInt16)_data);
            }
            else if(size <= 32)
            {
                writer.Write((UInt32)_data);
            }
            else
            {
                writer.Write(_data);
            }
        }

        public static bool NullOrGet(Bitset dirty)
        {
            return dirty == null || dirty.Get(); 
        }
    }
}
