using System;
using SharpNav;
using SharpNav.Geometry;
using SharpNav.Pathfinding;
using SocialPoint.IO;

namespace SocialPoint.Pathfinding
{
    public class NavTileSerializer : SimpleWriteSerializer<NavTile>
    {
        public static readonly NavTileSerializer Instance = new NavTileSerializer();

        public override void Serialize(NavTile value, IWriter writer)
        {
            NavVector2iSerializer.Instance.Serialize(value.Location, writer);
            writer.Write(value.Layer);
            writer.Write(value.Salt);
            NavBBox3Serializer.Instance.Serialize(value.Bounds, writer);

            SerializationUtils.SerializeArray<NavPoly>(value.Polys, NavPolySerializer.Instance.Serialize, writer);
            SerializationUtils.SerializeArray<Vector3>(value.Verts, NavVector3Serializer.Instance.Serialize, writer);

            SerializationUtils.SerializeArray<PolyMeshDetail.MeshData>(value.DetailMeshes, NavDetailMeshDataSerializer.Instance.Serialize, writer);
            SerializationUtils.SerializeArray<Vector3>(value.DetailVerts, NavVector3Serializer.Instance.Serialize, writer);
            SerializationUtils.SerializeArray<PolyMeshDetail.TriangleData>(value.DetailTris, NavDetailTriangleDataSerializer.Instance.Serialize, writer);

            SerializationUtils.SerializeArray<OffMeshConnection>(value.OffMeshConnections, NavOffMeshConnSerializer.Instance.Serialize, writer);

            writer.Write(value.BVTree.Count);
            for(int i = 0; i < value.BVTree.Count; i++)
            {
                NavBVTreeNodeSerializer.Instance.Serialize(value.BVTree[i], writer);
            }

            writer.Write(value.BvQuantFactor);
            writer.Write(value.BvNodeCount);
            writer.Write(value.WalkableClimb);
        }
    }

    public class NavTileParser : SimpleReadParser<NavTile>
    {
        public static readonly NavTileParser Instance = new NavTileParser();

        public override NavTile Parse(IReader reader)
        {
            return null;
        }
    }
}
