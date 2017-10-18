using System.Collections.Generic;
using System;
using System.Diagnostics;

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
            _serializers[code] = new TypeSerializer<K>(serializer);
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

        public bool TrySerialize(T obj, IWriter writer, bool writeCode = true)
        {
            byte code;
            if(FindCode(obj, out code))
            {
                ITypeSerializer serializer;
                if(_serializers.TryGetValue(code, out serializer))
                {
                    if(writeCode)
                    {
                        writer.Write(code);
                    }
                    serializer.Serialize(obj, writer);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool TrySerializeRaw(T obj, IWriter writer)
        {
            return TrySerialize(obj, writer, false);
        }

        public void SerializeRaw(T obj, IWriter writer)
        {
            if(!TrySerializeRaw(obj, writer))
            {
                throw new InvalidOperationException("No valid serializer found");
            }
        }

        public void Serialize(T obj, IWriter writer)
        {
            if(!TrySerialize(obj, writer))
            {
                throw new InvalidOperationException(string.Format("No valid serializer found {0}", obj));
            }
        }

        public bool FindCode(T obj, out byte code)
        {
            return _types.TryGetValue(obj.GetType(), out code);
        }

        public bool FindCode<K>(out byte code)
        {
            return _types.TryGetValue(typeof(K), out code);
        }
    }

    public class TypedWriteSerializer : TypedWriteSerializer<object>
    {
    }
}
