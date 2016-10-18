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
            SerializationUtils.SerializeArray<Link>(value.Links.ToArray(), NavLinkSerializer.Instance.Serialize, writer);
            SerializationUtils.SerializeIntArray(value.Verts, writer);
            SerializationUtils.SerializeIntArray(value.Neis, writer);
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
            var parsedLinks = SerializationUtils.ParseArray<Link>(NavLinkParser.Instance.Parse, reader);
            for(int i = 0; i < parsedLinks.Length; i++)
            {
                navPoly.Links.Add(parsedLinks[i]);
            }
            navPoly.Verts = SerializationUtils.ParseIntArray(reader);
            navPoly.Neis = SerializationUtils.ParseIntArray(reader);
            navPoly.VertCount = reader.ReadInt32();
            navPoly.Area = new SharpNav.Area(reader.ReadByte());
            //TODO: Parse NavPoly.Tag if used
            return navPoly;
        }
    }
}