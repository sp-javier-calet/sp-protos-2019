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
    public class NavMeshSerializer
    {
        IAttrSerializer _serializer;
        IAttrParser _parser;

        public NavMeshSerializer(IAttrSerializer serializer, IAttrParser parser)
        {
            _serializer = serializer;
            _parser = parser;
        }

        public void Serialize(string path, TiledNavMesh mesh)
        {
            var root = new AttrDic();

            root.Set("origin", AttrVector3Converter.Serialize(mesh.Origin));
            root.SetValue("tileWidth", mesh.TileWidth);
            root.SetValue("tileHeight", mesh.TileHeight);
            root.SetValue("maxTiles", mesh.MaxTiles);
            root.SetValue("maxPolys", mesh.MaxPolys);

            var tilesArray = new AttrList();
            foreach(NavTile tile in mesh.Tiles)
            {
                NavPolyId id = mesh.GetTileRef(tile);
                tilesArray.Add(SerializeMeshTile(tile, id));
            }
            root.Set("tiles", tilesArray);
			
            var data = _serializer.SerializeString(root);
            FileUtils.WriteAllText(path, data);
        }

        public TiledNavMesh Deserialize(string path)
        {
            string data = FileUtils.ReadAllText(path);
            var root = _parser.ParseString(data).AsDic;

            Vector3 origin = AttrVector3Converter.Parse(root["origin"]);
            float tileWidth = root["tileWidth"].AsValue.ToFloat();
            float tileHeight = root["tileHeight"].AsValue.ToFloat();
            int maxTiles = root["maxTiles"].AsValue.ToInt();
            int maxPolys = root["maxPolys"].AsValue.ToInt();

            var mesh = new TiledNavMesh(origin, tileWidth, tileHeight, maxTiles, maxPolys);

            var tilesArray = root["tiles"].AsList.ToList<AttrDic>();
            foreach(var tileToken in tilesArray)
            {
                NavPolyId tileRef;
                NavTile tile = DeserializeMeshTile(tileToken, mesh.IdManager, out tileRef);
                mesh.AddTileAt(tile, tileRef);
            }

            return mesh;
        }

        AttrDic SerializeMeshTile(NavTile tile, NavPolyId id)
        {
            var result = new AttrDic();

            result.SetValue("polyId", id.Id);
            result.Set("location", AttrVector2iConverter.Serialize(tile.Location));
            result.SetValue("layer", tile.Layer);
            result.SetValue("salt", tile.Salt);
            result.Set("bounds", AttrBBox3Converter.Serialize(tile.Bounds));
            result.Set("polys", SerializationUtils.Array2Attr<NavPoly>(tile.Polys, AttrNavPolyConverter.Serialize));
            /*result.Add("verts", tile.Verts);
            result.Add("detailMeshes", tile.DetailMeshes);
            result.Add("detailVerts", tile.DetailVerts);
            result.Add("detailTris", tile.DetailTris);
            result.Add("offMeshConnections", tile.OffMeshConnections);

            JObject treeObject = new JObject();
            JArray treeNodes = new JArray();
            for(int i = 0; i < tile.BVTree.Count; i++)
            {
                treeNodes.Add(tile.BVTree[i]);
            }
            treeObject.Add("nodes", treeNodes);

            result.Add("bvTree", treeObject);*/
            result.SetValue("bvQuantFactor", tile.BvQuantFactor);
            result.SetValue("bvNodeCount", tile.BvNodeCount);
            result.SetValue("walkableClimb", tile.WalkableClimb);

            return result;
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
