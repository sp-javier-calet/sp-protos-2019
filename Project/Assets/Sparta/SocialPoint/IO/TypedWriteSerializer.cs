using System.Collections.Generic;
using System;

namespace SocialPoint.IO
{
    public class TypedWriteSerializer<T> : IWriteSerializer<T>
    {
        interface ITypeSerializer
        {
            byte Code{ get; }
            bool Serialize(object obj, IWriter writer);
        }

        class TypeSerializer<K> : ITypeSerializer
        {
            IWriteSerializer<K> _serializer;

            public byte Code{ get; private set; }

            public TypeSerializer(byte code, IWriteSerializer<K> serializer)
            {
                Code = code;
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

        Dictionary<Type, ITypeSerializer> _serializers = new Dictionary<Type, ITypeSerializer>();

        public void Register<K>(byte code) where K : INetworkShareable
        {
            Register<K>(code, new NetworkShareableSerializer<K>());
        }

        public void Register<K>(byte code, IWriteSerializer<K> serializer)
        {
            _serializers[typeof(K)] = new TypeSerializer<K>(code, serializer);
        }

        public void Unregister<K>()
        {
            _serializers.Remove(typeof(K));
        }

        public void Serialize(T obj, IWriter writer)
        {
            var itr = _serializers.GetEnumerator();
            while(itr.MoveNext())
            {
                var s = itr.Current.Value;
                if(s.Serialize(obj, writer))
                {
                    break;
                }
            }
            itr.Dispose();
        }

        public bool FindCode(T obj, out byte code)
        {
            code = 0;
            var found = false;
            var itr = _serializers.GetEnumerator();
            while(itr.MoveNext())
            {
                if(itr.Current.Key.IsAssignableFrom(obj.GetType()))
                {
                    code = itr.Current.Value.Code;
                    found = true;
                    break;
                }
            }
            itr.Dispose();
            return found;
        }
    }

    public class TypedWriteSerializer : TypedWriteSerializer<object>
    {
    }
}
