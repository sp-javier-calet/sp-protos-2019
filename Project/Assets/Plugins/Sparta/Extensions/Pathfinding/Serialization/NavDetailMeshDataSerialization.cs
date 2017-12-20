using SharpNav;
using SocialPoint.IO;

namespace SocialPoint.Pathfinding
{
    public class NavDetailMeshDataSerializer : IWriteSerializer<PolyMeshDetail.MeshData>
    {
        public static readonly NavDetailMeshDataSerializer Instance = new NavDetailMeshDataSerializer();

        public void Serialize(PolyMeshDetail.MeshData value, IWriter writer)
        {
            writer.Write(value.VertexIndex);
            writer.Write(value.VertexCount);
            writer.Write(value.TriangleIndex);
            writer.Write(value.TriangleCount);
        }
    }

    public class NavDetailMeshDataParser : IReadParser<PolyMeshDetail.MeshData>
    {
        public static readonly NavDetailMeshDataParser Instance = new NavDetailMeshDataParser();

        public PolyMeshDetail.MeshData Parse(IReader reader)
        {
            var meshData = new PolyMeshDetail.MeshData();
            meshData.VertexIndex = reader.ReadInt32();
            meshData.VertexCount = reader.ReadInt32();
            meshData.TriangleIndex = reader.ReadInt32();
            meshData.TriangleCount = reader.ReadInt32();
            return meshData;
        }
    }
}

