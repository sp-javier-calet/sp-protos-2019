using NUnit.Framework;
using System.IO;
using System;
using System.Collections.Generic;
using SocialPoint.IO;
using Jitter.LinearMath;
using Jitter.Collision;
using Jitter.Collision.Shapes;

namespace SocialPoint.Physics
{
    [TestFixture]
    [Category("SocialPoint.Physics")]
    class SerializationTests
    {

        [Test]
        public void Vector()
        {
            SerializationTestUtils<JVector>.CompleteAndDifference(
                new JVector(1.0f, 2.3f, 4.2f),
                new JVector(1.0f, 3.3f, 4.2f),
                new JVectorSerializer(),
                new JVectorParser());
        }

        [Test]
        public void Quaternion()
        {
            SerializationTestUtils<JQuaternion>.CompleteAndDifference(
                new JQuaternion(1.0f, 2.3f, 4.2f, 5.0f),
                new JQuaternion(1.0f, 3.3f, 4.2f, 6.0f),
                new JQuaternionSerializer(),
                new JQuaternionParser());
        }

        [Test]
        public void Matrix()
        {
            SerializationTestUtils<JMatrix>.CompleteAndDifference(
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
        public void Shape()
        {
            SerializationTestUtils<IPhysicsShape>.Complete(
                new PhysicsBoxShape(new JVector(4.2f, 999.9f, 0.0f)));
            
            SerializationTestUtils<IPhysicsShape>.Complete(
                new PhysicsCapsuleShape(4.3f, 20.0f));

            SerializationTestUtils<IPhysicsShape>.Complete(
                new PhysicsCylinderShape(4.3f, 20.0f));

            SerializationTestUtils<IPhysicsShape>.Complete(
                new PhysicsSphereShape(2.5f));

            SerializationTestUtils<IPhysicsShape>.Complete(
                new PhysicsMeshShape(new List<JVector>{
                    new JVector(2.434f, 2324.342f, 9568.0f),
                    new JVector(1f, 2f, 4f),
                    new JVector(2f, 1f, 40f),
                    new JVector(2f, 1f, 40f)
                }, new List<TriangleVertexIndices>{
                    new TriangleVertexIndices(0, 1, 2),
                    new TriangleVertexIndices(2, 3, 0)
                }));
        }
    }
}
