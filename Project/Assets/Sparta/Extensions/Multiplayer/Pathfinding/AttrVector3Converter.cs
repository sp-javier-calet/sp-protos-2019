using System;
using SharpNav.Geometry;
using SocialPoint.Attributes;

namespace SocialPoint.Pathfinding
{
    public class AttrVector3Converter
    {
        public static Attr Serialize(Vector3 value)
        {
            var attr = new AttrDic();
            attr.SetValue("X", value.X);
            attr.SetValue("Y", value.Y);
            attr.SetValue("Z", value.Z);
            return attr;
        }

        public static Vector3 Parse(Attr attr)
        {
            var dic = attr.AsDic;
            float x = dic.Get("X").AsValue.ToFloat();
            float y = dic.Get("Y").AsValue.ToFloat();
            float z = dic.Get("Z").AsValue.ToFloat();
            return new Vector3(x, y, z);
        }
    }
}
