using System;
using SharpNav.Pathfinding;
using SocialPoint.Attributes;
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
            //attr.Set(kVerts, SUtils.Array2Attr(value.Verts));
            //attr.Set(kNeis, SUtils.Array2Attr(value.Neis));
            //attr.SetValue(kTag, value.Tag);
            //TODO: Complete
        }
    }

    public class NavPolyParser : SimpleReadParser<NavPoly>
    {
        public static readonly NavPolyParser Instance = new NavPolyParser();

        public override NavPoly Parse(IReader reader)
        {
            var navPoly = new NavPoly();
            /*navPoly.PolyType = (NavPolyType)dic[kPolyType].AsValue.ToInt();
            var parsedLinks = SUtils.Attr2Array<Link>(dic[kLinks], AttrLinkConverter.Parse);
            for(int i = 0; i < parsedLinks.Length; i++)
            {
                navPoly.Links.Add(parsedLinks[i]);
            }
            navPoly.Verts = SUtils.Attr2ArrayInt(dic[kVerts]);*/
            //TODO: Complete
            return navPoly;
        }
    }
}