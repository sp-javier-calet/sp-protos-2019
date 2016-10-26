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

            T Parse(IReader reader)
            {
                return _parser.Parse(reader);
            }
        }

        Dictionary<Type, byte> _types = new Dictionary<Type, byte>();
        Dictionary<byte, ITypeParser> _parsers = new Dictionary<byte, ITypeParser>();

        public void Register<K>(byte code) where K : INetworkShareable, new()
        {
            Register<K>(code, new NetworkShareableParser<K>());
        }

        public void Register<K>(byte code, IReadParser<K> parser)
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

        public bool TryParse(byte code, IReader reader, out T obj)
        {
        }

        public T Parse(byte code, IReader reader)
        {
            ITypeParser parser;
            if(_parsers.TryGetValue(code, out parser))
            {
                return parser.Parse(reader);
            }
            throw new InvalidOperationException("No valid parser found");
        }
    }

    public class TypedReadParser : TypedReadParser<object>
    {
    }
}
