using SharpNav.Pathfinding;
using SocialPoint.IO;

namespace SocialPoint.Pathfinding
{
    public class NavPolySerializer : IWriteSerializer<NavPoly>
    {
        public static readonly NavPolySerializer Instance = new NavPolySerializer();

        public void Serialize(NavPoly value, IWriter writer)
        {
            writer.Write((int)value.PolyType);
            writer.WriteArray<Link>(value.Links.ToArray(), NavLinkSerializer.Instance.Serialize);
            writer.WriteInt32Array(value.Verts);
            writer.WriteInt32Array(value.Neis);
            writer.Write(value.VertCount);   
            writer.Write(value.Area.Id);
            writer.Write(value.Flags);
            NavTagSerializer.Instance.Serialize(value.Tag, writer);
        }
    }

    public class NavPolyParser : IReadParser<NavPoly>
    {
        public static readonly NavPolyParser Instance = new NavPolyParser();

        public NavPoly Parse(IReader reader)
        {
            var navPoly = new NavPoly();
            navPoly.PolyType = (NavPolyType)reader.ReadInt32();
            var parsedLinks = reader.ReadArray<Link>(NavLinkParser.Instance.Parse);
            for(int i = 0; i < parsedLinks.Length; i++)
            {
                navPoly.Links.Add(parsedLinks[i]);
            }
            navPoly.Verts = reader.ReadInt32Array();
            navPoly.Neis = reader.ReadInt32Array();
            navPoly.VertCount = reader.ReadInt32();
            navPoly.Area = new SharpNav.Area(reader.ReadByte());
            navPoly.Flags = reader.ReadUInt16();
            navPoly.Tag = NavTagParser.Instance.Parse(reader);
            return navPoly;
        }
    }
}
