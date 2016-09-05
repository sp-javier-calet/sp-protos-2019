using NUnit.Framework;
using System.IO;
using System;
using SocialPoint.IO;
using SocialPoint.Network;
using BulletSharp.Math;

namespace SocialPoint.Multiplayer
{
    [TestFixture]
    [Category("SocialPoint.Multiplayer")]
    class SerializationTests
    {
        void GenericInitial<T>(T obj, IWriteSerializer<T> serializer, IReadParser<T> parser)
        {
            var stream = new MemoryStream();
            var writer = new SystemBinaryWriter(stream);
            serializer.Serialize(obj, writer);
            stream.Seek(0, SeekOrigin.Begin);
            var reader = new SystemBinaryReader(stream);
            var obj2 = parser.Parse(reader);
            Assert.That(obj.Equals(obj2));
        }

        void GenericDiff<T>(T newObj, T oldObj, IWriteSerializer<T> serializer, IReadParser<T> parser)
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
        public void Vector3Hash()
        {
            Vector3 v1 = new Vector3(1.0f, 2.0f, 3.0f);
            Vector3 v2 = new Vector3(1.0f, 3.0f, 2.0f);
            Vector3 v3 = new Vector3(1.0f, 3.0f, 2.0f);
            Assert.That(v1.GetHashCode() != v2.GetHashCode());
            Assert.That(v2.GetHashCode() == v3.GetHashCode());
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
        public void Vector4Hash()
        {
            Vector4 v1 = new Vector4(1.0f, 2.0f, 3.0f, 4.0f);
            Vector4 v2 = new Vector4(1.0f, 3.0f, 2.0f, 4.0f);
            Vector4 v3 = new Vector4(1.0f, 3.0f, 2.0f, 4.0f);
            Assert.That(v1.GetHashCode() != v2.GetHashCode());
            Assert.That(v2.GetHashCode() == v3.GetHashCode());
        }

        [Test]
        public void Vector4Initial()
        {
            GenericInitial(
                new Vector4(1.0f, 2.3f, 4.2f, 5.2f),
                new Vector4Serializer(),
                new Vector4Parser());
        }

        [Test]
        public void Vector4Diff()
        {
            GenericDiff(
                new Vector4(1.0f, 2.3f, 4.2f, 5.2f),
                new Vector4(1.0f, 3.3f, 4.2f, 6.2f),
                new Vector4Serializer(),
                new Vector4Parser());
        }

        [Test]
        public void QuaternionHash()
        {
            Quaternion q1 = new Quaternion(1.0f, 2.0f, 3.0f, 4.0f);
            Quaternion q2 = new Quaternion(1.0f, 3.0f, 2.0f, 4.0f);
            Quaternion q3 = new Quaternion(1.0f, 3.0f, 2.0f, 4.0f);
            Assert.That(q1.GetHashCode() != q2.GetHashCode());
            Assert.That(q2.GetHashCode() == q3.GetHashCode());
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
        public void MatrixHash()
        {
            Matrix m1 = new Matrix(
                            1.0f, 2.0f, 3.0f, 4.0f,
                            1.0f, 2.0f, 3.0f, 4.0f,
                            1.0f, 2.0f, 3.0f, 4.0f,
                            1.0f, 2.0f, 3.0f, 4.0f);
            Matrix m2 = new Matrix(
                            4.0f, 2.0f, 3.0f, 4.0f,
                            1.0f, 3.0f, 3.0f, 4.0f,
                            1.0f, 2.0f, 2.0f, 4.0f,
                            1.0f, 2.0f, 3.0f, 1.0f);
            Matrix m3 = new Matrix(
                            4.0f, 2.0f, 3.0f, 4.0f,
                            1.0f, 3.0f, 3.0f, 4.0f,
                            1.0f, 2.0f, 2.0f, 4.0f,
                            1.0f, 2.0f, 3.0f, 1.0f);
            Assert.That(m1.GetHashCode() != m2.GetHashCode());
            Assert.That(m2.GetHashCode() == m3.GetHashCode());
        }

        [Test]
        public void MatrixInitial()
        {
            GenericInitial(
                new Matrix(
                    1.0f, 2.3f, 4.2f, 5.2f,
                    3.0f, 2.3f, 7.2f, 5.2f,
                    4.0f, 4.5f, 4.5f, 15f,
                    5.0f, 2.4f, 4.2f, 52.2f),
                new MatrixSerializer(),
                new MatrixParser());
        }

        [Test]
        public void MatrixDiff()
        {
            GenericDiff(
                new Matrix(
                    1.0f, 2.3f, 4.2f, 5.2f,
                    3.0f, 2.3f, 7.2f, 5.2f,
                    4.0f, 4.5f, 4.5f, 15f,
                    5.0f, 2.4f, 4.2f, 52.2f),
                new Matrix(
                    1.0f, 2.3f, 4.2f, 5.2f,
                    3.0f, 2.3f, 7.2f, 5.2f,
                    4.0f, 5.5f, 4.5f, 15f,
                    5.0f, 2.4f, 4.2f, 52.2f),
                new MatrixSerializer(),
                new MatrixParser());
        }

        [Test]
        public void TransformHash()
        {
            Transform t1 = new Transform(
                               new Vector3(1.0f, 2.0f, 3.0f),
                               new Quaternion(1.0f, 2.0f, 3.0f, 4.0f),
                               new Vector3(1.0f, 1.0f, 2.0f));
            Transform t2 = new Transform(
                               new Vector3(1.0f, 3.0f, 2.0f),
                               new Quaternion(1.0f, 3.0f, 2.0f, 4.0f),
                               new Vector3(2.0f, 1.0f, 1.0f));
            Transform t3 = new Transform(
                               new Vector3(1.0f, 3.0f, 2.0f),
                               new Quaternion(1.0f, 3.0f, 2.0f, 4.0f),
                               new Vector3(2.0f, 1.0f, 1.0f));
            Assert.That(t1.GetHashCode() != t2.GetHashCode());
            Assert.That(t2.GetHashCode() == t3.GetHashCode());
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

        [Test]
        public void NetworkGameObjectInitial()
        {
            GenericInitial(
                new NetworkGameObject(1, new Transform(
                    new Vector3(1.0f, 2.3f, 4.2f),
                    new Quaternion(1.0f, 2.3f, 4.2f, 5.0f),
                    new Vector3(2.0f, 1.0f, 2.0f)
                )
                ),
                new NetworkGameObjectSerializer(),
                new NetworkGameObjectParser());
        }

        [Test]
        public void NetworkGameObjectDiff()
        {
            GenericDiff(
                new NetworkGameObject(1, new Transform(
                    new Vector3(1.0f, 2.3f, 4.2f),
                    new Quaternion(1.0f, 2.3f, 4.2f, 5.0f),
                    new Vector3(2.0f, 1.0f, 2.0f)
                )
                ),
                new NetworkGameObject(1, new Transform(
                    new Vector3(1.0f, 3.3f, 4.2f),
                    new Quaternion(1.0f, 3.3f, 4.2f, 6.0f),
                    new Vector3(1.0f, 0.0f, 2.0f)
                )
                ),
                new NetworkGameObjectSerializer(),
                new NetworkGameObjectParser());
        }

        [Test]
        public void NetworkGameSceneInitial()
        {
            var scene = new NetworkScene();

            scene.AddObject(new NetworkGameObject(1, new Transform(
                new Vector3(1.0f, 2.3f, 4.2f),
                new Quaternion(1.0f, 2.3f, 4.2f, 5.0f),
                new Vector3(2.0f, 1.0f, 2.0f)
            )));

            scene.AddObject(new NetworkGameObject(2, new Transform(
                new Vector3(2.0f, 2.3f, 4.2f),
                new Quaternion(5.0f, 2.3f, 4.2f, 5.0f),
                new Vector3(3.0f, 1.0f, 2.0f)
            )));

            GenericInitial(scene,
                new NetworkSceneSerializer(),
                new NetworkSceneParser());
        }

        [Test]
        public void NetworkGameSceneAddDiff()
        {
            var scene = new NetworkScene();

            scene.AddObject(new NetworkGameObject(1, new Transform(
                new Vector3(1.0f, 2.3f, 4.2f),
                new Quaternion(1.0f, 2.3f, 4.2f, 5.0f),
                new Vector3(2.0f, 1.0f, 2.0f)
            )));

            var scene2 = new NetworkScene();

            scene2.AddObject(new NetworkGameObject(1, new Transform(
                new Vector3(1.0f, 2.3f, 4.6f),
                new Quaternion(3.0f, 2.3f, 4.2f, 5.0f),
                new Vector3(2.0f, 1.0f, 4.0f)
            )));

            scene2.AddObject(new NetworkGameObject(2, new Transform(
                new Vector3(2.0f, 2.3f, 4.2f),
                new Quaternion(5.0f, 2.3f, 4.2f, 5.0f),
                new Vector3(3.0f, 1.0f, 2.1f)
            )));

            GenericDiff(scene2, scene,
                new NetworkSceneSerializer(),
                new NetworkSceneParser());
        }

        [Test]
        public void NetworkGameSceneRemoveDiff()
        {
            var scene = new NetworkScene();

            scene.AddObject(new NetworkGameObject(1, new Transform(
                new Vector3(1.0f, 2.3f, 4.2f),
                new Quaternion(1.0f, 2.3f, 4.2f, 5.0f),
                new Vector3(2.0f, 1.0f, 2.0f)
            )));

            scene.AddObject(new NetworkGameObject(2, new Transform(
                new Vector3(2.0f, 2.3f, 4.2f),
                new Quaternion(5.0f, 2.3f, 4.2f, 5.0f),
                new Vector3(3.0f, 1.0f, 2.1f)
            )));

            var scene2 = new NetworkScene();

            scene2.AddObject(new NetworkGameObject(1, new Transform(
                new Vector3(1.0f, 2.3f, 4.6f),
                new Quaternion(3.0f, 2.3f, 4.2f, 5.0f),
                new Vector3(2.0f, 1.0f, 4.0f)
            )));

            GenericDiff(scene2, scene,
                new NetworkSceneSerializer(),
                new NetworkSceneParser());
        }
    }
}
