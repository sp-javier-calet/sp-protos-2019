using System;
using SharpNav.Pathfinding;
using SocialPoint.Attributes;
using SUtils = SocialPoint.Pathfinding.SerializationUtils;

namespace SocialPoint.Pathfinding
{
    public static class AttrNavPolyConverter
    {
        const string kPolyType = "PolyType";
        const string kLinks = "Links";
        const string kVerts = "Verts";
        const string kNeis = "Neis";
        const string kTag = "Tag";
        const string kVertCount = "VertCount";
        const string kArea = "Area";

        public static Attr Serialize(NavPoly value)
        {
            var attr = new AttrDic();
            attr.SetValue(kPolyType, (int)value.PolyType);
            attr.Set(kLinks, SUtils.Array2Attr(value.Links.ToArray(), AttrLinkConverter.Serialize));
            attr.Set(kVerts, SUtils.Array2Attr(value.Verts));
            //TODO: Complete
            return attr;
        }

        public static NavPoly Parse(Attr attr)
        {
            var dic = attr.AsDic;
            var navPoly = new NavPoly();
            navPoly.PolyType = (NavPolyType)dic[kPolyType].AsValue.ToInt();
            var parsedLinks = SUtils.Attr2Array<Link>(dic[kLinks], AttrLinkConverter.Parse);
            for(int i = 0; i < parsedLinks.Length; i++)
            {
                navPoly.Links.Add(parsedLinks[i]);
            }
            navPoly.Verts = SUtils.Attr2ArrayInt(dic[kVerts]);
            //TODO: Complete
            return navPoly;
        }
    }
}

