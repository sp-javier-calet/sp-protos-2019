using NUnit.Framework;
using System.IO;
using System;
using SocialPoint.IO;
using SocialPoint.Network;
using SocialPoint.Physics;
using Jitter.LinearMath;

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

        void GenericDiff<T>(T newObj, T oldObj, IDiffWriteSerializer<T> serializer, IDiffReadParser<T> parser)
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
                new JVector(1.0f, 2.3f, 4.2f),
                new JVectorSerializer(),
                new JVectorParser());
        }

        [Test]
        public void Vector3Diff()
        {
            GenericDiff(
                new JVector(1.0f, 2.3f, 4.2f),
                new JVector(1.0f, 3.3f, 4.2f),
                new JVectorSerializer(),
                new JVectorParser());
        }

        [Test]
        public void QuaternionInitial()
        {
            GenericInitial(
                new JQuaternion(1.0f, 2.3f, 4.2f, 5.0f),
                new JQuaternionSerializer(),
                new JQuaternionParser());
        }

        [Test]
        public void QuaternionDiff()
        {
            GenericDiff(
                new JQuaternion(1.0f, 2.3f, 4.2f, 5.0f),
                new JQuaternion(1.0f, 3.3f, 4.2f, 6.0f),
                new JQuaternionSerializer(),
                new JQuaternionParser());
        }

        [Test]
        public void MatrixInitial()
        {
            GenericInitial(
                new JMatrix(
                    1.0f, 2.3f, 4.2f,
                    3.0f, 2.3f, 7.2f,
                    4.0f, 4.5f, 4.5f),
                new JMatrixSerializer(),
                new JMatrixParser());
        }

        [Test]
        public void MatrixDiff()
        {
            GenericDiff(
                new JMatrix(
                    1.0f, 2.3f, 4.2f,
                    3.0f, 2.3f, 7.2f,
                    4.0f, 4.5f, 4.5f),
                new JMatrix(
                    1.0f, 2.3f, 4.2f,
                    3.0f, 2.3f, 7.2f,
                    4.0f, 5.5f, 4.5f),
                new JMatrixSerializer(),
                new JMatrixParser());
        }

        [Test]
        public void TransformInitial()
        {
            GenericInitial(
                new Transform(
                    new JVector(1.0f, 2.3f, 4.2f),
                    new JQuaternion(1.0f, 2.3f, 4.2f, 5.0f),
                    new JVector(2.0f, 1.0f, 2.0f)
                ),
                new TransformSerializer(),
                new TransformParser());
        }

        [Test]
        public void TransformDiff()
        {
            GenericDiff(
                new Transform(
                    new JVector(1.0f, 2.3f, 4.2f),
                    new JQuaternion(1.0f, 2.3f, 4.2f, 5.0f),
                    new JVector(2.0f, 1.0f, 2.0f)
                ),
                new Transform(
                    new JVector(1.0f, 3.3f, 4.2f),
                    new JQuaternion(1.0f, 3.3f, 4.2f, 6.0f),
                    new JVector(1.0f, 0.0f, 2.0f)
                ),
                new TransformSerializer(),
                new TransformParser());
        }

        [Test]
        public void NetworkGameObjectInitial()
        {
            var g0 = new NetworkGameObject();
            g0.Init(1, false, new Transform(
                new JVector(1.0f, 2.3f, 4.2f),
                new JQuaternion(1.0f, 2.3f, 4.2f, 5.0f),
                new JVector(2.0f, 1.0f, 2.0f)
            )
            );
            GenericInitial(
                g0,
                new NetworkGameObjectSerializer(),
                new NetworkGameObjectParser());
        }

        [Test]
        public void NetworkGameObjectDiff()
        {
            var g0 = new NetworkGameObject();
            g0.Init(1, false, new Transform(
                new JVector(1.0f, 2.3f, 4.2f),
                new JQuaternion(1.0f, 2.3f, 4.2f, 5.0f),
                new JVector(2.0f, 1.0f, 2.0f)
            )
            );
            var g1 = new NetworkGameObject();
            g1.Init(1, false, new Transform(
                new JVector(1.0f, 3.3f, 4.2f),
                new JQuaternion(1.0f, 3.3f, 4.2f, 6.0f),
                new JVector(1.0f, 0.0f, 2.0f)
            )
            );
            GenericDiff(g0, g1, new NetworkGameObjectSerializer(), new NetworkGameObjectParser());
        }

        [Test]
        public void NetworkGameSceneInitial()
        {
            var scene = new NetworkScene();
            var g0 = new NetworkGameObject();
            g0.Init(1, false, new Transform(
                new JVector(1.0f, 2.3f, 4.2f),
                new JQuaternion(1.0f, 2.3f, 4.2f, 5.0f),
                new JVector(2.0f, 1.0f, 2.0f)
            )
            );
            scene.AddObject(g0);

            var g1 = new NetworkGameObject();
            g1.Init(2, false, new Transform(
                new JVector(2.0f, 2.3f, 4.2f),
                new JQuaternion(5.0f, 2.3f, 4.2f, 5.0f),
                new JVector(3.0f, 1.0f, 2.0f)
            )
            );
            scene.AddObject(g1);

            GenericInitial(scene,
                new NetworkSceneSerializer(),
                new NetworkSceneParser());
        }

        [Test]
        public void NetworkGameSceneAddDiff()
        {
            var scene = new NetworkScene();

            var g0 = new NetworkGameObject();
            g0.Init(1, false, new Transform(
                new JVector(1.0f, 2.3f, 4.2f),
                new JQuaternion(1.0f, 2.3f, 4.2f, 5.0f),
                new JVector(2.0f, 1.0f, 2.0f)
            )
            );
            scene.AddObject(g0);

            var scene2 = new NetworkScene();

            var g1 = new NetworkGameObject();
            g1.Init(1, false, new Transform(
                new JVector(1.0f, 2.3f, 4.6f),
                new JQuaternion(3.0f, 2.3f, 4.2f, 5.0f),
                new JVector(2.0f, 1.0f, 4.0f)
            )
            );
            scene2.AddObject(g1);

            var g2 = new NetworkGameObject();
            g2.Init(2, false, new Transform(
                new JVector(2.0f, 2.3f, 4.2f),
                new JQuaternion(5.0f, 2.3f, 4.2f, 5.0f),
                new JVector(3.0f, 1.0f, 2.1f)
            )
            );
            scene2.AddObject(g2);

            GenericDiff(scene2, scene,
                new NetworkSceneSerializer(),
                new NetworkSceneParser());
        }

        [Test]
        public void NetworkGameSceneRemoveDiff()
        {
            var scene = new NetworkScene();

            var g0 = new NetworkGameObject();
            g0.Init(1, false, new Transform(
                new JVector(1.0f, 2.3f, 4.2f),
                new JQuaternion(1.0f, 2.3f, 4.2f, 5.0f),
                new JVector(2.0f, 1.0f, 2.0f)
            )
            );
            scene.AddObject(g0);

            var g1 = new NetworkGameObject();
            g1.Init(2, false, new Transform(
                new JVector(2.0f, 2.3f, 4.2f),
                new JQuaternion(5.0f, 2.3f, 4.2f, 5.0f),
                new JVector(3.0f, 1.0f, 2.1f)
            )
            );
            scene.AddObject(g1);

            var scene2 = new NetworkScene();

            var g2 = new NetworkGameObject();
            g2.Init(1, false, new Transform(
                new JVector(1.0f, 2.3f, 4.6f),
                new JQuaternion(3.0f, 2.3f, 4.2f, 5.0f),
                new JVector(2.0f, 1.0f, 4.0f)
            )
            );
            scene2.AddObject(g2);

            GenericDiff(scene2, scene,
                new NetworkSceneSerializer(),
                new NetworkSceneParser());
        }
    }
}
