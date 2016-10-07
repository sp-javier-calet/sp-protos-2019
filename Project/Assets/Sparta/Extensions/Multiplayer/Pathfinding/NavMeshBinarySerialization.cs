using System;
using System.Collections.Generic;
using SharpNav;
using SharpNav.Collections;
using SharpNav.Geometry;
using SharpNav.Pathfinding;
using SocialPoint.IO;

/// <summary>
/// Class based in the original files from SharpNav/IO/Json.
/// </summary>
namespace SocialPoint.Pathfinding
{
    public class NavMeshBinarySerializer : SimpleWriteSerializer<TiledNavMesh>
    {
        public static readonly NavMeshBinarySerializer Instance = new NavMeshBinarySerializer();

        public override void Serialize(TiledNavMesh mesh, IWriter writer)
        {
            NavVector3Serializer.Instance.Serialize(mesh.Origin, writer);
            writer.Write(mesh.TileWidth);
            writer.Write(mesh.TileHeight);
            writer.Write(mesh.MaxTiles);
            writer.Write(mesh.MaxPolys);

            writer.Write(mesh.TileCount);
            var itr = mesh.Tiles.GetEnumerator();
            while(itr.MoveNext())
            {
                var tile = itr.Current;
                NavPolyId id = mesh.GetTileRef(tile);

                writer.Write(id.Id);
                SerializeTile(tile, writer);
            }
            itr.Dispose();
        }

        void SerializeTile(NavTile value, IWriter writer)
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

    public class NavMeshBinaryParser : SimpleReadParser<TiledNavMesh>
    {
        public static readonly NavMeshBinaryParser Instance = new NavMeshBinaryParser();

        public override TiledNavMesh Parse(IReader reader)
        {
            Vector3 origin = NavVector3Parser.Instance.Parse(reader);
            float tileWidth = reader.ReadSingle();
            float tileHeight = reader.ReadSingle();
            int maxTiles = reader.ReadInt32();
            int maxPolys = reader.ReadInt32();

            var mesh = new TiledNavMesh(origin, tileWidth, tileHeight, maxTiles, maxPolys);

            int tileCount = reader.ReadInt32();
            for(int i = 0; i < tileCount; i++)
            {
                NavPolyId tileRef = new NavPolyId(reader.ReadInt32());
                NavTile tile = ParseTile(mesh.IdManager, tileRef, reader);
                mesh.AddTileAt(tile, tileRef);
            }

            return mesh;
        }

        NavTile ParseTile(NavPolyIdManager manager, NavPolyId refId, IReader reader)
        {
            Vector2i location = NavVector2iParser.Instance.Parse(reader);
            int layer = reader.ReadInt32();
            NavTile result = new NavTile(location, layer, manager, refId);

            result.Salt = reader.ReadInt32();
            result.Bounds = NavBBox3Parser.Instance.Parse(reader);

            result.Polys = SerializationUtils.ParseArray<NavPoly>(NavPolyParser.Instance.Parse, reader);
            result.PolyCount = result.Polys.Length;
            result.Verts = SerializationUtils.ParseArray<Vector3>(NavVector3Parser.Instance.Parse, reader);

            result.DetailMeshes = SerializationUtils.ParseArray<PolyMeshDetail.MeshData>(NavDetailMeshDataParser.Instance.Parse, reader);
            result.DetailVerts = SerializationUtils.ParseArray<Vector3>(NavVector3Parser.Instance.Parse, reader);
            result.DetailTris = SerializationUtils.ParseArray<PolyMeshDetail.TriangleData>(NavDetailTriangleDataParser.Instance.Parse, reader);
            result.OffMeshConnections = SerializationUtils.ParseArray<OffMeshConnection>(NavOffMeshConnParser.Instance.Parse, reader);
            result.OffMeshConnectionCount = result.OffMeshConnections.Length;

            var nodes = SerializationUtils.ParseArray<BVTree.Node>(NavBVTreeNodeParser.Instance.Parse, reader);
            result.BVTree = new BVTree(nodes);

            result.BvQuantFactor = reader.ReadSingle();
            result.BvNodeCount = reader.ReadInt32();
            result.WalkableClimb = reader.ReadSingle();

            return result;
        }
    }
}
