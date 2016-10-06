using System;
using SharpNav.Pathfinding;
using SocialPoint.Attributes;

namespace SocialPoint.Pathfinding
{
    public class AttrNavPolyConverter
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
            //TODO: Complete
            return attr;
        }

        public static NavPoly Parse(Attr attr)
        {
            var dic = attr.AsDic;
            var navPoly = new NavPoly();
            navPoly.PolyType = (NavPolyType)dic[kPolyType].AsValue.ToInt();
            //TODO: Complete
            return navPoly;
        }
    }
}

