using SharpNav.Geometry;
using SocialPoint.Geometry;
using SocialPoint.IO;

namespace SocialPoint.Pathfinding
{
    public class NavVector3Serializer : IWriteSerializer<Vector3>
    {
        public static readonly NavVector3Serializer Instance = new NavVector3Serializer();

        NavVector3Serializer()
        {
        }

        public void Serialize(Vector3 value, IWriter writer)
        {
            VectorSerializer.Instance.Serialize(value, writer);
        }
    }

    public class NavVector3Parser : IReadParser<Vector3>
    {
        public static readonly NavVector3Parser Instance = new NavVector3Parser();

        NavVector3Parser()
        {
        }

        public Vector3 Parse(IReader reader)
        {
            return VectorParser.Instance.Parse(reader);
        }
    }
}
