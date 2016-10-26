using System.Collections.Generic;
using System;

namespace SocialPoint.IO
{
    public class TypedWriteSerializer<T> : IWriteSerializer<T>
    {
        interface ITypeSerializer
        {
            bool Serialize(object obj, IWriter writer);
        }

        class TypeSerializer<K> : ITypeSerializer
        {
            IWriteSerializer<K> _serializer;

            public TypeSerializer(IWriteSerializer<K> serializer)
            {  
                _serializer = serializer;
            }

            public bool Serialize(object obj, IWriter writer)
            {
                if(!(obj is K))
                {
                    return false;
                }
                var tobj = (K)obj;
                _serializer.Serialize(tobj, writer);
                return true;
            }
        }

        Dictionary<Type, byte> _types = new Dictionary<Type, byte>();
        Dictionary<byte, ITypeSerializer> _serializers = new Dictionary<byte, ITypeSerializer>();

        public void Register<K>(byte code) where K : INetworkShareable
        {
            Register<K>(code, new NetworkShareableSerializer<K>());
        }

        public void Register<K>(byte code, IWriteSerializer<K> serializer)
        {
            _types[typeof(K)] = code;
            _serializers[code] = new TypeSerializer<K>(code, serializer);
        }

        public void Unregister<K>()
        {
            var type = typeof(K);
            byte code;
            if(_types.TryGetValue(type, out code))
            {
                _serializers.Remove(code);
            }
        }

        public void Serialize(T obj, IWriter writer)
        {
            bool found = false;
            var itr = _serializers.GetEnumerator();
            while(itr.MoveNext())
            {
                var s = itr.Current.Value;
                if(s.Serialize(obj, writer))
                {
                    found = true;
                    break;
                }
            }
            itr.Dispose();
            if(!found)
            {
                throw new InvalidOperationException("No valid serializer found");
            }
        }

        public bool FindCode(T obj, out byte code)
        {
            return _types.TryGetValue(obj, out code);
        }
    }

    public class TypedWriteSerializer : TypedWriteSerializer<object>
    {
    }
}
