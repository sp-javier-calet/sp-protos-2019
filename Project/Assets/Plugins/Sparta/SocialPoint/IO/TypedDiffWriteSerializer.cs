using System.Collections.Generic;
using System;

namespace SocialPoint.IO
{
    public class TypedDiffWriteSerializer<T> : IDiffWriteSerializer<T>
    {
        interface ITypeDiffSerializer
        {
            bool Serialize(object obj, IWriter writer);

            bool Serialize(object newObj, object oldObj, IWriter writer);
        }

        class TypeDiffSerializer<K> : ITypeDiffSerializer
        {
            IDiffWriteSerializer<K> _serializer;

            public TypeDiffSerializer(IDiffWriteSerializer<K> serializer)
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

            public bool Serialize(object newObj, object oldObj, IWriter writer)
            {
                if(!(newObj is K) || !(oldObj is K))
                {
                    return false;
                }
                var newK = (K)newObj;
                var oldK = (K)oldObj;
                _serializer.Serialize(newK, oldK, writer);
                return true;
            }
        }

        Dictionary<Type, byte> _types = new Dictionary<Type, byte>();
        Dictionary<byte, ITypeDiffSerializer> _serializers = new Dictionary<byte, ITypeDiffSerializer>();

        public void Register<K>(byte code, IDiffWriteSerializer<K> serializer)
        {
            _types[typeof(K)] = code;
            _serializers[code] = new TypeDiffSerializer<K>(serializer);
        }

        public void Unregister<K>()
        {
            byte code;
            if(FindCode<K>(out code))
            {
                _serializers.Remove(code);
            }
        }

        public bool TrySerialize(T obj, IWriter writer, bool writeCode=true)
        {
            byte code;
            if(FindCode(obj, out code))
            {
                ITypeDiffSerializer serializer;
                if(_serializers.TryGetValue(code, out serializer))
                {
                    if(writeCode)
                    {
                        writer.Write(code);
                    }
                    serializer.Serialize(obj, writer);
                    return true;
                }
            }
            return false;
        }

        public bool TrySerializeRaw(T obj, IWriter writer)
        {
            return TrySerialize(obj, writer, false);
        }

        public bool TrySerialize(T newObj, T oldObj, IWriter writer, bool writeCode=true)
        {
            byte newCode, oldCode;
            if(FindCode(newObj, out newCode) && FindCode(oldObj, out oldCode))
            {
                ITypeDiffSerializer serializer;
                if(newCode == oldCode && _serializers.TryGetValue(newCode, out serializer))
                {
                    if(writeCode)
                    {
                        writer.Write(newCode);
                    }
                    serializer.Serialize(newObj, oldObj, writer);
                    return true;
                }
            }
            return false;
        }

        public bool TrySerializeRaw(T newObj, T oldObj, IWriter writer)
        {
            return TrySerialize(newObj, oldObj, writer, false);
        }

        public bool TrySerializeTyped(T obj, Type type, IWriter writer, bool writeCode=true)
        {
            byte code;
            if(FindCode(type, out code))
            {
                ITypeDiffSerializer serializer;
                if(_serializers.TryGetValue(code, out serializer))
                {
                    if(writeCode)
                    {
                        writer.Write(code);
                    }
                    serializer.Serialize(obj, writer);
                    return true;
                }
            }
            return false;
        }

        public bool TrySerializeTyped(T newObj, Type newType, T oldObj, Type oldType, IWriter writer, bool writeCode=true)
        {
            byte newCode, oldCode;
            if(FindCode(newType, out newCode) && FindCode(oldType, out oldCode))
            {
                ITypeDiffSerializer serializer;
                if(newCode == oldCode && _serializers.TryGetValue(newCode, out serializer))
                {
                    if(writeCode)
                    {
                        writer.Write(newCode);
                    }
                    serializer.Serialize(newObj, oldObj, writer);
                    return true;
                }
            }
            return false;
        }

        public void Serialize(T obj, IWriter writer)
        {
            if(!TrySerialize(obj, writer))
            {
                throw new InvalidOperationException("No valid serializer found");
            }
        }

        public void Serialize(T newObj, T oldObj, IWriter writer, Bitset dirty)
        {
            if(!TrySerialize(newObj, oldObj, writer))
            {
                throw new InvalidOperationException("No valid serializer found");
            }
        }

        public void SerializeTyped(T obj, Type type, IWriter writer)
        {
            if(!TrySerializeTyped(obj, type, writer))
            {
                throw new InvalidOperationException("No valid serializer found");
            }
        }

        public void SerializeTyped(T newObj, Type newType, T oldObj, Type oldType, IWriter writer, Bitset dirty)
        {
            if(!TrySerializeTyped(newObj, newType, oldObj, oldType, writer))
            {
                throw new InvalidOperationException("No valid serializer found");
            }
        }

        public void SerializeRaw(T obj, IWriter writer)
        {
            if(!TrySerializeRaw(obj, writer))
            {
                throw new InvalidOperationException("No valid serializer found");
            }
        }

        public void SerializeRaw(T newObj, T oldObj, IWriter writer, Bitset dirty)
        {
            if(!TrySerializeRaw(newObj, oldObj, writer))
            {
                throw new InvalidOperationException("No valid serializer found");
            }
        }

        public void Compare(T newObj, T oldObj, Bitset dirty)
        {
        }

        public bool FindCode(T obj, out byte code)
        {
            return _types.TryGetValue(obj.GetType(), out code);
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

    public class TypedDiffWriteSerializer : TypedDiffWriteSerializer<object>
    {
    }
}
