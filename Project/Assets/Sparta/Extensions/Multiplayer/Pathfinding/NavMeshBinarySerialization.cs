using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SharpNav;
using SharpNav.Collections;
using SharpNav.Geometry;
using SharpNav.Pathfinding;
using System.Reflection;
using SocialPoint.Attributes;
using SocialPoint.IO;

/// <summary>
/// Class based in the original files from SharpNav/IO/Json.
/// </summary>
namespace SocialPoint.Pathfinding
{
    public class NavMeshBinarySerializer : SimpleWriteSerializer<TiledNavMesh>
    {
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
                NavTileSerializer.Instance.Serialize(tile, writer);
            }
            itr.Dispose();
        }
    }

    public class NavMeshBinaryParser : SimpleReadParser<TiledNavMesh>
    {
        public override TiledNavMesh Parse(IReader reader)
        {
            Vector3 origin = NavVector3Parser.Instance.Parse(reader);
            float tileWidth = reader.ReadSingle();
            float tileHeight = reader.ReadSingle();
            int maxTiles = reader.ReadInt32();
            int maxPolys = reader.ReadInt32();

            var mesh = new TiledNavMesh(origin, tileWidth, tileHeight, maxTiles, maxPolys);

            /*int tileCount = reader.ReadInt32();
            for(int i = 0; i < tileCount; i++)
            {
                NavPolyId tileRef = new NavPolyId(reader.ReadInt32());
                NavTile tile = DeserializeMeshTile(tileToken, mesh.IdManager, out tileRef);
                mesh.AddTileAt(tile, tileRef);
            }*/

            return mesh;
        }

        NavTile DeserializeMeshTile(AttrDic token, NavPolyIdManager manager, out NavPolyId refId)
        {
            refId = new NavPolyId(token["polyId"].AsValue.ToInt());
            Vector2i location = AttrVector2iConverter.Parse(token["location"]);
            int layer = token["layer"].AsValue.ToInt();
            NavTile result = new NavTile(location, layer, manager, refId);

            result.Salt = token["salt"].AsValue.ToInt();
            result.Bounds = AttrBBox3Converter.Parse(token["bounds"]);
            result.Polys = SerializationUtils.Attr2Array<NavPoly>(token["polys"], AttrNavPolyConverter.Parse);
            /*result.PolyCount = result.Polys.Length;
            result.Verts = token["verts"].ToObject<Vector3[]>(serializer);
            result.DetailMeshes = token["detailMeshes"].ToObject<PolyMeshDetail.MeshData[]>(serializer);
            result.DetailVerts = token["detailVerts"].ToObject<Vector3[]>(serializer);
            result.DetailTris = token["detailTris"].ToObject<PolyMeshDetail.TriangleData[]>(serializer);
            result.OffMeshConnections = token["offMeshConnections"].ToObject<OffMeshConnection[]>(serializer);
            result.OffMeshConnectionCount = result.OffMeshConnections.Length;
            result.BvNodeCount = token["bvNodeCount"].AsValue.ToInt();
            result.BvQuantFactor = token["bvQuantFactor"].AsValue.ToFloat();
            result.WalkableClimb = token["walkableClimb"].AsValue.ToFloat();
    
            var treeObject = (JObject)token["bvTree"];
            var nodes = treeObject.GetValue("nodes").ToObject<BVTree.Node[]>();

            result.BVTree = new BVTree(nodes);*/

            return result;
        }
    }
}
