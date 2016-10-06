using System;
using SharpNav.Geometry;
using SocialPoint.Attributes;

namespace SocialPoint.Pathfinding
{
    public class AttrVector2iConverter
    {
        public static Attr Serialize(Vector2i value)
        {
            var attr = new AttrDic();
            attr.SetValue("X", value.X);
            attr.SetValue("Y", value.Y);
            return attr;
        }

        public static Vector2i Parse(Attr attr)
        {
            var dic = attr.AsDic;
            int x = dic.Get("X").AsValue.ToInt();
            int y = dic.Get("Y").AsValue.ToInt();
            return new Vector2i(x, y);
        }
    }
}
