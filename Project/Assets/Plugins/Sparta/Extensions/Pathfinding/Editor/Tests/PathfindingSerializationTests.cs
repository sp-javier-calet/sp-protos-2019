using NUnit.Framework;
using System.IO;
using System;
using SocialPoint.IO;
using SocialPoint.Network;
using SharpNav;
using SharpNav.Geometry;
using SharpNav.Collections;
using SharpNav.Pathfinding;
using SocialPoint.Pathfinding;

namespace SocialPoint.Pathfinding
{
    [TestFixture]
    [Category("SocialPoint.Pathfinding")]
    class PathfindingSerializationTests
    {
        void GenericTest<T>(T obj1, T obj2, IWriteSerializer<T> serializer, IReadParser<T> parser)
        {
            var stream = new MemoryStream();
            var writer = new SystemBinaryWriter(stream);
            serializer.Serialize(obj1, writer);
            serializer.Serialize(obj2, writer);

            stream.Seek(0, SeekOrigin.Begin);
            var reader = new SystemBinaryReader(stream);
            var newObj1 = parser.Parse(reader);
            var newObj2 = parser.Parse(reader);

            Assert.That(!obj1.Equals(obj2));
            Assert.That(obj1.Equals(newObj1));
            Assert.That(obj2.Equals(newObj2));
        }

        [Test]
        public void Vector3Test()
        {
            GenericTest(
                new Vector3(1.0f, 2.3f, 4.2f),
                new Vector3(1.0f, 4.2f, 2.3f),
                new NavVector3Serializer(),
                new NavVector3Parser());
        }

        [Test]
        public void Vector2iTest()
        {
            GenericTest(
                new Vector2i(1, 2),
                new Vector2i(2, 1),
                new NavVector2iSerializer(),
                new NavVector2iParser());
        }

        [Test]
        public void PolyVertexTest()
        {
            GenericTest(
                new PolyVertex(1, 2, 3),
                new PolyVertex(1, 3, 2),
                new NavPolyVertexSerializer(),
                new NavPolyVertexParser());
        }

        [Test]
        public void BBox3Test()
        {
            var vMin1 = new Vector3(-1.0f, -2.3f, -4.5f);
            var vMax1 = new Vector3(1.0f, 2.3f, 4.5f);
            var vMin2 = new Vector3(-1.0f, -4.5f, -2.3f);
            var vMax2 = new Vector3(1.0f, 4.5f, 2.3f);
            GenericTest(
                new BBox3(vMin1, vMax1),
                new BBox3(vMin2, vMax2),
                new NavBBox3Serializer(),
                new NavBBox3Parser());
        }

        [Test]
        public void PolyBoundsTest()
        {
            var vMin1 = new PolyVertex(-1, -2, -4);
            var vMax1 = new PolyVertex(1, 2, 4);
            var vMin2 = new PolyVertex(-1, -4, -2);
            var vMax2 = new PolyVertex(1, 4, 2);
            GenericTest(
                new PolyBounds(vMin1, vMax1),
                new PolyBounds(vMin2, vMax2),
                new NavPolyBoundsSerializer(),
                new NavPolyBoundsParser());
        }

        [Test]
        public void BVTreeNodeTest()
        {
            var vMin1 = new PolyVertex(-1, -2, -4);
            var vMax1 = new PolyVertex(1, 2, 4);
            var vMin2 = new PolyVertex(-1, -4, -2);
            var vMax2 = new PolyVertex(1, 4, 2);
            var b1 = new PolyBounds(vMin1, vMax1);
            var b2 = new PolyBounds(vMin2, vMax2);

            var value1 = new BVTree.Node();
            var value2 = new BVTree.Node();
            value1.Bounds = b1;
            value2.Bounds = b2;
            value1.Index = 1;
            value2.Index = 2;

            //BVTree.Node is struct, Equals does a bitwise comparison
            GenericTest(
                value1,
                value2,
                new NavBVTreeNodeSerializer(),
                new NavBVTreeNodeParser());
        }

        [Test]
        public void DetailMeshDataTest()
        {
            var vi1 = 1;
            var vc1 = 2;
            var ti1 = 3;
            var tc1 = 4;
            var vi2 = 4;
            var vc2 = 3;
            var ti2 = 2;
            var tc2 = 1;

            var value1 = new PolyMeshDetail.MeshData();
            var value2 = new PolyMeshDetail.MeshData();
            value1.TriangleCount = tc1;
            value1.TriangleIndex = ti1;
            value1.VertexCount = vc1;
            value1.VertexIndex = vi1;
            value2.TriangleCount = tc2;
            value2.TriangleIndex = ti2;
            value2.VertexCount = vc2;
            value2.VertexIndex = vi2;

            //PolyMeshDetail.MeshData is struct, Equals does a bitwise comparison
            GenericTest(
                value1,
                value2,
                new NavDetailMeshDataSerializer(),
                new NavDetailMeshDataParser());
        }

        [Test]
        public void DetailTriangleDataTest()
        {
            var f1 = 1;
            var h21 = 2;
            var h11 = 3;
            var h01 = 4;
            var f2 = 4;
            var h22 = 3;
            var h12 = 2;
            var h02 = 1;

            var value1 = new PolyMeshDetail.TriangleData();
            var value2 = new PolyMeshDetail.TriangleData();
            value1.VertexHash0 = h01;
            value1.VertexHash1 = h11;
            value1.VertexHash2 = h21;
            value1.Flags = f1;
            value2.VertexHash0 = h02;
            value2.VertexHash1 = h12;
            value2.VertexHash2 = h22;
            value2.Flags = f2;

            //PolyMeshDetail.TriangleData is struct, Equals does a bitwise comparison
            GenericTest(
                value1,
                value2,
                new NavDetailTriangleDataSerializer(),
                new NavDetailTriangleDataParser());
        }

        //TODO: implement IEquatable interfaces for other classes we require to Test here
    }
}
