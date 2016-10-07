using System;
using SharpNav.Pathfinding;
using SocialPoint.Attributes;
using SocialPoint.IO;

namespace SocialPoint.Pathfinding
{
    public class NavLinkSerializer : SimpleWriteSerializer<Link>
    {
        public static readonly NavLinkSerializer Instance = new NavLinkSerializer();

        public override void Serialize(Link value, IWriter writer)
        {
            writer.Write(value.Reference.Id);
            writer.Write(value.Edge);
            writer.Write((int)value.Side);
            writer.Write(value.BMin);
            writer.Write(value.BMax);
        }
    }

    public class NavLinkParser : SimpleReadParser<Link>
    {
        public static readonly NavLinkParser Instance = new NavLinkParser();

        public override Link Parse(IReader reader)
        {
            var link = new Link();
            link.Reference = new NavPolyId(reader.ReadInt32());
            link.Edge = reader.ReadInt32();
            link.Side = (BoundarySide)reader.ReadInt32();
            link.BMin = reader.ReadInt32();
            link.BMax = reader.ReadInt32();
            return link;
        }
    }
}