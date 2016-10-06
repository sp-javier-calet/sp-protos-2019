using System;
using SharpNav.Geometry;
using SocialPoint.Attributes;

namespace SocialPoint.Pathfinding
{
    public class AttrBBox3Converter
    {
        public static Attr Serialize(BBox3 value)
        {
            var attr = new AttrDic();
            attr.Set("Min", AttrVector3Converter.Serialize(value.Min));
            attr.Set("Max", AttrVector3Converter.Serialize(value.Max));
            return attr;
        }

        public static BBox3 Parse(Attr attr)
        {
            var dic = attr.AsDic;
            Vector3 min = AttrVector3Converter.Parse(dic.Get("Min"));
            Vector3 max = AttrVector3Converter.Parse(dic.Get("Max"));
            return new BBox3(min, max);
        }
    }
}
