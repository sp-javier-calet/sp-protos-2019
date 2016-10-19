using SharpNav.Pathfinding;
using SocialPoint.IO;

namespace SocialPoint.Pathfinding
{
    public class NavPolySerializer : SimpleWriteSerializer<NavPoly>
    {
        public static readonly NavPolySerializer Instance = new NavPolySerializer();

        public override void Serialize(NavPoly value, IWriter writer)
        {
            writer.Write((int)value.PolyType);
            writer.Write<Link>(value.Links.ToArray(), NavLinkSerializer.Instance.Serialize);
            writer.Write(value.Verts);
            writer.Write(value.Neis);
            writer.Write(value.VertCount);   
            writer.Write(value.Area.Id);
            //TODO: Serialize NavPoly.Tag if used
        }
    }

    public class NavPolyParser : SimpleReadParser<NavPoly>
    {
        public static readonly NavPolyParser Instance = new NavPolyParser();

        public override NavPoly Parse(IReader reader)
        {
            var navPoly = new NavPoly();
            navPoly.PolyType = (NavPolyType)reader.ReadInt32();
            var parsedLinks = reader.Read<Link>(NavLinkParser.Instance.Parse);
            for(int i = 0; i < parsedLinks.Length; i++)
            {
                navPoly.Links.Add(parsedLinks[i]);
            }
            navPoly.Verts = reader.ReadInt32Array();
            navPoly.Neis = reader.ReadInt32Array();
            navPoly.VertCount = reader.ReadInt32();
            navPoly.Area = new SharpNav.Area(reader.ReadByte());
            //TODO: Parse NavPoly.Tag if used
            return navPoly;
        }
    }
}