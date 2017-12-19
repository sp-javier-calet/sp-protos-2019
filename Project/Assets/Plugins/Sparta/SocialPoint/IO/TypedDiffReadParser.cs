using System.Collections.Generic;
using System;

namespace SocialPoint.IO
{
    public class TypedDiffReadParser<T> : IDiffReadParser<T>
    {
        public interface ITypeDiffParser
        {
            T Parse(IReader reader);

            T Parse(object oldObj, IReader reader);
        }

        class TypeDiffParser<K> : ITypeDiffParser where K : T
        {
            IDiffReadParser<K> _parser;

            public TypeDiffParser(byte code, IDiffReadParser<K> parser)
            {
                _parser = parser;
            }

            public T Parse(IReader reader)
            {
                return _parser.Parse(reader);
            }

            public T Parse(object oldObj, IReader reader)
            {
                if(!(oldObj is K))
                {
                    throw new InvalidOperationException("No valid parser found");
                }
                var oldK = (K)oldObj;
                return _parser.Parse<K>(oldK, reader);
            }
        }

        Dictionary<Type, byte> _types = new Dictionary<Type, byte>();
        Dictionary<byte, ITypeDiffParser> _parsers = new Dictionary<byte, ITypeDiffParser>();

        public Dictionary<byte, ITypeDiffParser> Parsers
        {
            get
            {
                return _parsers;
            }
        }

        public void Register<K>(byte code, IDiffReadParser<K> parser) where K : T
        {
            _parsers[code] = new TypeDiffParser<K>(code, parser);
            _types[typeof(K)] = code;
        }

        public void Unregister<K>()
        {
            var type = typeof(K);
            byte code;
            if(_types.TryGetValue(type, out code))
            {
                _parsers.Remove(code);
            }
        }

        public bool TryParse(byte code, IReader reader, out T obj)
        {
            ITypeDiffParser parser;
            if(_parsers.TryGetValue(code, out parser))
            {
                obj = parser.Parse(reader);
                return true;
            }
            obj = default(T);
            return false;
        }

        public bool TryParse(byte code, T oldObj, IReader reader, out T obj)
        {
            ITypeDiffParser parser;
            if(_parsers.TryGetValue(code, out parser))
            {
                obj = parser.Parse(oldObj, reader);
                return true;
            }
            obj = default(T);
            return false;
        }

        public T Parse(byte code, IReader reader)
        {
            T obj;
            if(TryParse(code, reader, out obj))
            {
                return obj;
            }
            throw new InvalidOperationException("No valid parser found");
        }

        public T Parse(byte code, T oldObj, IReader reader)
        {
            T obj;
            if(TryParse(code, oldObj, reader, out obj))
            {
                return obj;
            }
            throw new InvalidOperationException("No valid parser found");
        }

        public T Parse(T oldObj, IReader reader)
        {
            var code = reader.ReadByte();
            return Parse(code, oldObj, reader);
        }

        public T Parse(IReader reader)
        {
            var code = reader.ReadByte();
            return Parse(code, reader);
        }

        public T Parse(T oldObj, IReader reader, Bitset dirty)
        {
            var code = reader.ReadByte();
            return Parse(code, oldObj, reader);
        }

        public int GetDirtyBitsSize(T obj)
        {
            return 0;
        }

        public bool FindCode(Type type, out byte code)
        {
            return _types.TryGetValue(type, out code);
        }

        public bool FindCode<K>(out byte code)
        {
            return _types.TryGetValue(typeof(K), out code);
        }
    }

    public class TypedDiffReadParser : TypedDiffReadParser<object>
    {
    }
}
