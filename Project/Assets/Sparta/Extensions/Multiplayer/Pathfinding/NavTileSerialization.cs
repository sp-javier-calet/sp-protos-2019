using System;
using SharpNav.Pathfinding;
using SocialPoint.Attributes;
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

            /*result.Set("location", AttrVector2iConverter.Serialize(tile.Location));
            result.SetValue("layer", tile.Layer);
            result.SetValue("salt", tile.Salt);
            result.Set("bounds", AttrBBox3Converter.Serialize(tile.Bounds));
            result.Set("polys", SerializationUtils.Array2Attr<NavPoly>(tile.Polys, AttrNavPolyConverter.Serialize));
            result.Add("verts", tile.Verts);
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

            result.Add("bvTree", treeObject);
            result.SetValue("bvQuantFactor", tile.BvQuantFactor);
            result.SetValue("bvNodeCount", tile.BvNodeCount);
            result.SetValue("walkableClimb", tile.WalkableClimb);*/
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
