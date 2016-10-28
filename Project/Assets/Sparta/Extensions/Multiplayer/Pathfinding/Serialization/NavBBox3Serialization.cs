using SharpNav.Geometry;
using SocialPoint.IO;

namespace SocialPoint.Pathfinding
{
    public class NavBBox3Serializer : SimpleWriteSerializer<BBox3>
    {
        public static readonly NavBBox3Serializer Instance = new NavBBox3Serializer();

        public override void Serialize(BBox3 value, IWriter writer)
        {
            NavVector3Serializer.Instance.Serialize(value.Min, writer);
            NavVector3Serializer.Instance.Serialize(value.Max, writer);
        }
    }

    public class NavBBox3Parser : SimpleReadParser<BBox3>
    {
        public static readonly NavBBox3Parser Instance = new NavBBox3Parser();

        public override BBox3 Parse(IReader reader)
        {
            Vector3 min = NavVector3Parser.Instance.Parse(reader);
            Vector3 max = NavVector3Parser.Instance.Parse(reader);
            return new BBox3(min, max);
        }
    }
}
