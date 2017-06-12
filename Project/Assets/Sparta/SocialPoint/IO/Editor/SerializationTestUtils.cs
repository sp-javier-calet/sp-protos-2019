using NUnit.Framework;
using System.IO;
using System;

namespace SocialPoint.IO
{
    static class SerializationTestUtils<T>
    {
        static bool Compare(T obj1, T obj2, Func<T, T, bool> comparer)
        {
            if(comparer == null)
            {
                return obj1.Equals(obj2);
            }
            else
            {
                return comparer(obj1, obj2);
            }
        }

        static bool Compare<K>(K obj1, K obj2, Func<K, K, bool> comparer) where K : T
        {
            if(comparer == null)
            {
                return obj1.Equals(obj2);
            }
            else
            {
                return comparer(obj1, obj2);
            }
        }

        public static void Complete<K>(K obj, Func<K, K, bool> comparer = null) where K : T, INetworkShareable, new()
        {
            var stream = new MemoryStream();
            var writer = new SystemBinaryWriter(stream);
            obj.Serialize(writer);
            stream.Seek(0, SeekOrigin.Begin);
            var reader = new SystemBinaryReader(stream);
            var obj2 = new K();
            obj2.Deserialize(reader);
            Assert.That(Compare(obj, obj2, comparer));
        }

        public static void Complete(T obj, IWriteSerializer<T> serializer, IReadParser<T> parser, Func<T, T, bool> comparer = null)
        {
            var stream = new MemoryStream();
            var writer = new SystemBinaryWriter(stream);
            serializer.Serialize(obj, writer);
            stream.Seek(0, SeekOrigin.Begin);
            var reader = new SystemBinaryReader(stream);
            var obj2 = parser.Parse(reader);
            Assert.That(Compare(obj, obj2, comparer));
        }

        public static void Difference(T newObj, T oldObj, IDiffWriteSerializer<T> serializer, IDiffReadParser<T> parser, Func<T, T, bool> comparer = null)
        {
            var stream = new MemoryStream();
            var writer = new SystemBinaryWriter(stream);
            serializer.Serialize(newObj, oldObj, writer);
            stream.Seek(0, SeekOrigin.Begin);
            var reader = new SystemBinaryReader(stream);
            var newObj2 = parser.Parse(oldObj, reader);
            Assert.That(Compare(newObj, newObj2, comparer));
        }

        public static void CompleteAndDifference(T newObj, T oldObj, IDiffWriteSerializer<T> serializer, IDiffReadParser<T> parser)
        {
            Complete(newObj, serializer, parser);
            Difference(newObj, oldObj, serializer, parser);
        }
    }
}
