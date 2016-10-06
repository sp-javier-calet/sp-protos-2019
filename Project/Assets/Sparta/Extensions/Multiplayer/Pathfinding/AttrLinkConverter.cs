using System;
using SharpNav.Pathfinding;
using SocialPoint.Attributes;

namespace SocialPoint.Pathfinding
{
    public static class AttrLinkConverter
    {
        const string kReference = "Reference";
        const string kEdge = "Edge";
        const string kSide = "Side";
        const string kBMin = "BMin";
        const string kBMax = "BMax";

        public static Attr Serialize(Link value)
        {
            var attr = new AttrDic();
            attr.SetValue(kReference, value.Reference.Id);
            attr.SetValue(kEdge, value.Edge);
            attr.SetValue(kSide, (int)value.Side);
            attr.SetValue(kBMin, value.BMin);
            attr.SetValue(kBMax, value.BMax);
            return attr;
        }

        public static Link Parse(Attr attr)
        {
            var dic = attr.AsDic;
            var link = new Link();
            link.Reference = new NavPolyId(dic[kReference].AsValue.ToInt());
            link.Edge = dic[kEdge].AsValue.ToInt();
            link.Side = (BoundarySide)dic[kSide].AsValue.ToInt();
            link.BMin = dic[kBMin].AsValue.ToInt();
            link.BMax = dic[kBMax].AsValue.ToInt();
            return link;
        }
    }
}

