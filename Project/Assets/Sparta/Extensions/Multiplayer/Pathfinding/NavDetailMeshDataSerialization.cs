using System;
using SharpNav;
using SocialPoint.Attributes;
using SocialPoint.IO;

namespace SocialPoint.Pathfinding
{
    public class NavDetailMeshDataSerializer : SimpleWriteSerializer<PolyMeshDetail.MeshData>
    {
        public static readonly NavDetailMeshDataSerializer Instance = new NavDetailMeshDataSerializer();

        public override void Serialize(PolyMeshDetail.MeshData value, IWriter writer)
        {
            writer.Write(value.VertexIndex);
            writer.Write(value.VertexCount);
            writer.Write(value.TriangleIndex);
            writer.Write(value.TriangleCount);
        }
    }

    public class NavDetailMeshDataParser : SimpleReadParser<PolyMeshDetail.MeshData>
    {
        public static readonly NavDetailMeshDataParser Instance = new NavDetailMeshDataParser();

        public override PolyMeshDetail.MeshData Parse(IReader reader)
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

