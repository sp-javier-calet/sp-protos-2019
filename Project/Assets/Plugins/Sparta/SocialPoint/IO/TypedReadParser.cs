using System.Collections.Generic;
using System;

namespace SocialPoint.IO
{
    public class TypedReadParser<T> : IReadParser<T>
    {
        interface ITypeParser
        {
            T Parse(IReader reader);
        }

        class TypeParser<K> : ITypeParser where K : T
        {
            IReadParser<K> _parser;

            public TypeParser(byte code, IReadParser<K> parser)
            {
                _parser = parser;
            }

            public T Parse(IReader reader)
            {
                return _parser.Parse(reader);
            }
        }

        Dictionary<Type, byte> _types = new Dictionary<Type, byte>();
        Dictionary<byte, ITypeParser> _parsers = new Dictionary<byte, ITypeParser>();

        public void Register<K>(byte code) where K : T, INetworkShareable, new()
        {
            Register<K>(code, new NetworkShareableParser<K>());
        }

        public void Register<K>(byte code, IReadParser<K> parser) where K : T
        {
            _parsers[code] = new TypeParser<K>(code, parser);
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

        public bool TryParseRaw(byte code, IReader reader, out T obj)
        {
            ITypeParser parser;
            if(_parsers.TryGetValue(code, out parser))
            {
                obj = parser.Parse(reader);
                return true;
            }
            obj = default(T);
            return false;
        }

        public T ParseRaw(byte code, IReader reader)
        {
            T obj;
            if(TryParseRaw(code, reader, out obj))
            {
                return obj;
            }
            throw new InvalidOperationException("No valid parser found");
        }

        public T Parse(IReader reader)
        {
            var code = reader.ReadByte();
            return ParseRaw(code, reader);
        }
    }

    public class TypedReadParser : TypedReadParser<object>
    {
    }
}
