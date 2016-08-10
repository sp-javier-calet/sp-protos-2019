
using NUnit.Framework;
using System.IO;
using System;
using SocialPoint.IO;

namespace SocialPoint.Multiplayer
{
    [TestFixture]
    [Category("SocialPoint.Multiplayer")]
    class SerializationTests
    {
        void InitialObject<T>(T obj, ISerializer<T> serializer, IParser<T> parser)
        {
            var stream = new MemoryStream();
            var writer = new SystemBinaryWriter(stream);
            serializer.Serialize(obj, writer);
            stream.Seek(0, SeekOrigin.Begin);
            var reader = new SystemBinaryReader(stream);
            var obj2 = parser.Parse(reader);
            Assert.That(obj.Equals(obj2));
        }

        void DiffObject<T>(T newObj, T oldObj, ISerializer<T> serializer, IParser<T> parser)
        {
            var stream = new MemoryStream();
            var writer = new SystemBinaryWriter(stream);
            serializer.Serialize(newObj, oldObj, writer);
            stream.Seek(0, SeekOrigin.Begin);
            var reader = new SystemBinaryReader(stream);
            var newObj2 = parser.Parse(oldObj, reader);
            Assert.That(newObj.Equals(newObj2));
        }
            
        [Test]
        public void InitialVector3()
        {
            InitialObject(
                new Vector3(1.0f, 2.3f, 4.2f),
                new Vector3Serializer(),
                new Vector3Parser());
        }

        [Test]
        public void DiffVector3()
        {
            DiffObject(
                new Vector3(1.0f, 2.3f, 4.2f),
                new Vector3(1.0f, 3.3f, 4.2f),
                new Vector3Serializer(),
                new Vector3Parser());
        }
    }
}
