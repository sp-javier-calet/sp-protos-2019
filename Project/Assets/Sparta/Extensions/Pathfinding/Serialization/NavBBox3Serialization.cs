using SharpNav.Geometry;
using SocialPoint.Geometry;
using SocialPoint.IO;

namespace SocialPoint.Pathfinding
{
    public class NavBBox3Serializer : IWriteSerializer<BBox3>
    {
        public static readonly NavBBox3Serializer Instance = new NavBBox3Serializer();

        NavBBox3Serializer()
        {
        }

        public void Serialize(BBox3 value, IWriter writer)
        {
            VectorSerializer.Instance.Serialize(value.Min, writer);
            VectorSerializer.Instance.Serialize(value.Max, writer);
        }
    }

    public class NavBBox3Parser : IReadParser<BBox3>
    {
        public static readonly NavBBox3Parser Instance = new NavBBox3Parser();

        NavBBox3Parser()
        {
        }

        public BBox3 Parse(IReader reader)
        {
            var min = VectorParser.Instance.Parse(reader);
            var max = VectorParser.Instance.Parse(reader);
            return new BBox3(min, max);
        }
    }
}
