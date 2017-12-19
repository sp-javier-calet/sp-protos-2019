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
    public class NavMeshSerializer : IWriteSerializer<TiledNavMesh>
    {
        public static readonly NavMeshSerializer Instance = new NavMeshSerializer();

        public void Serialize(TiledNavMesh mesh, IWriter writer)
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

            writer.WriteArray<NavPoly>(value.Polys, NavPolySerializer.Instance.Serialize);
            writer.WriteArray<Vector3>(value.Verts, NavVector3Serializer.Instance.Serialize);

            writer.WriteArray<PolyMeshDetail.MeshData>(value.DetailMeshes, NavDetailMeshDataSerializer.Instance.Serialize);
            writer.WriteArray<Vector3>(value.DetailVerts, NavVector3Serializer.Instance.Serialize);
            writer.WriteArray<PolyMeshDetail.TriangleData>(value.DetailTris, NavDetailTriangleDataSerializer.Instance.Serialize);
            writer.WriteArray<OffMeshConnection>(value.OffMeshConnections, NavOffMeshConnSerializer.Instance.Serialize);

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

    public class NavMeshParser : IReadParser<TiledNavMesh>
    {
        public static readonly NavMeshParser Instance = new NavMeshParser();

        public TiledNavMesh Parse(IReader reader)
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

            result.Polys = reader.ReadArray<NavPoly>(NavPolyParser.Instance.Parse);
            result.PolyCount = result.Polys.Length;
            result.Verts = reader.ReadArray<Vector3>(NavVector3Parser.Instance.Parse);

            result.DetailMeshes = reader.ReadArray<PolyMeshDetail.MeshData>(NavDetailMeshDataParser.Instance.Parse);
            result.DetailVerts = reader.ReadArray<Vector3>(NavVector3Parser.Instance.Parse);
            result.DetailTris = reader.ReadArray<PolyMeshDetail.TriangleData>(NavDetailTriangleDataParser.Instance.Parse);
            result.OffMeshConnections = reader.ReadArray<OffMeshConnection>(NavOffMeshConnParser.Instance.Parse);
            result.OffMeshConnectionCount = result.OffMeshConnections.Length;

            var nodes = reader.ReadArray<BVTree.Node>(NavBVTreeNodeParser.Instance.Parse);
            result.BVTree = new BVTree(nodes);

            result.BvQuantFactor = reader.ReadSingle();
            result.BvNodeCount = reader.ReadInt32();
            result.WalkableClimb = reader.ReadSingle();

            return result;
        }
    }
}
