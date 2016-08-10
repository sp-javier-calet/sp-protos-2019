
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
        void GenericInitial<T>(T obj, ISerializer<T> serializer, IParser<T> parser)
        {
            var stream = new MemoryStream();
            var writer = new SystemBinaryWriter(stream);
            serializer.Serialize(obj, writer);
            stream.Seek(0, SeekOrigin.Begin);
            var reader = new SystemBinaryReader(stream);
            var obj2 = parser.Parse(reader);
            Assert.That(obj.Equals(obj2));
        }

        void GenericDiff<T>(T newObj, T oldObj, ISerializer<T> serializer, IParser<T> parser)
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
        public void Vector3Initial()
        {
            GenericInitial(
                new Vector3(1.0f, 2.3f, 4.2f),
                new Vector3Serializer(),
                new Vector3Parser());
        }

        [Test]
        public void Vector3Diff()
        {
            GenericDiff(
                new Vector3(1.0f, 2.3f, 4.2f),
                new Vector3(1.0f, 3.3f, 4.2f),
                new Vector3Serializer(),
                new Vector3Parser());
        }

        [Test]
        public void QuaternionInitial()
        {
            GenericInitial(
                new Quaternion(1.0f, 2.3f, 4.2f, 5.0f),
                new QuaternionSerializer(),
                new QuaternionParser());
        }

        [Test]
        public void QuaternionDiff()
        {
            GenericDiff(
                new Quaternion(1.0f, 2.3f, 4.2f, 5.0f),
                new Quaternion(1.0f, 3.3f, 4.2f, 6.0f),
                new QuaternionSerializer(),
                new QuaternionParser());
        }

        [Test]
        public void TransformInitial()
        {
            GenericInitial(
                new Transform(
                    new Vector3(1.0f, 2.3f, 4.2f),
                    new Quaternion(1.0f, 2.3f, 4.2f, 5.0f),
                    new Vector3(2.0f, 1.0f, 2.0f)
                ),
                new TransformSerializer(),
                new TransformParser());
        }

        [Test]
        public void TransformDiff()
        {
            GenericDiff(
                new Transform(
                    new Vector3(1.0f, 2.3f, 4.2f),
                    new Quaternion(1.0f, 2.3f, 4.2f, 5.0f),
                    new Vector3(2.0f, 1.0f, 2.0f)
                ),
                new Transform(
                    new Vector3(1.0f, 3.3f, 4.2f),
                    new Quaternion(1.0f, 3.3f, 4.2f, 6.0f),
                    new Vector3(1.0f, 0.0f, 2.0f)
                ),
                new TransformSerializer(),
                new TransformParser());
        }
    }
}
