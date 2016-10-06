using System;
using SharpNav.Pathfinding;
using SocialPoint.Attributes;

namespace SocialPoint.Pathfinding
{
    public static class AttrNavPolyConverter
    {
        const string kPolyType = "PolyType";
        const string kLinks = "Links";
        const string kVert = "Verts";
        const string kNeis = "Neis";
        const string kTag = "Tag";
        const string kVertCount = "VertCount";
        const string kArea = "Area";

        public static Attr Serialize(NavPoly value)
        {
            var attr = new AttrDic();
            attr.SetValue(kPolyType, (int)value.PolyType);
            attr.Set(kLinks, SerializationUtils.Array2Attr(value.Links.ToArray(), AttrLinkConverter.Serialize));
            //TODO: Complete
            return attr;
        }

        public static NavPoly Parse(Attr attr)
        {
            var dic = attr.AsDic;
            var navPoly = new NavPoly();
            navPoly.PolyType = (NavPolyType)dic[kPolyType].AsValue.ToInt();
            var parsedLinks = SerializationUtils.Attr2Array<Link>(dic[kLinks], AttrLinkConverter.Parse);
            for(int i = 0; i < parsedLinks.Length; i++)
            {
                navPoly.Links.Add(parsedLinks[i]);
            }
            //TODO: Complete
            return navPoly;
        }
    }
}

