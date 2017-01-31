using SharpNav;
using SocialPoint.IO;

namespace SocialPoint.Pathfinding
{
    public class NavDetailTriangleDataSerializer : IWriteSerializer<PolyMeshDetail.TriangleData>
    {
        public static readonly NavDetailTriangleDataSerializer Instance = new NavDetailTriangleDataSerializer();

        public void Serialize(PolyMeshDetail.TriangleData value, IWriter writer)
        {
            writer.Write(value.VertexHash0);
            writer.Write(value.VertexHash1);
            writer.Write(value.VertexHash2);
            writer.Write(value.Flags);
        }
    }

    public class NavDetailTriangleDataParser : IReadParser<PolyMeshDetail.TriangleData>
    {
        public static readonly NavDetailTriangleDataParser Instance = new NavDetailTriangleDataParser();

        public PolyMeshDetail.TriangleData Parse(IReader reader)
        {
            var meshData = new PolyMeshDetail.TriangleData();
            meshData.VertexHash0 = reader.ReadInt32();
            meshData.VertexHash1 = reader.ReadInt32();
            meshData.VertexHash2 = reader.ReadInt32();
            meshData.Flags = reader.ReadInt32();
            return meshData;
        }
    }
}

