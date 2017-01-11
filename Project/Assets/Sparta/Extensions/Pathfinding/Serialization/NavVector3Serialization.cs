using SharpNav.Geometry;
using SocialPoint.IO;

namespace SocialPoint.Pathfinding
{
    public class NavVector3Serializer : IWriteSerializer<Vector3>
    {
        public static readonly NavVector3Serializer Instance = new NavVector3Serializer();

        public void Serialize(Vector3 value, IWriter writer)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
        }
    }

    public class NavVector3Parser : IReadParser<Vector3>
    {
        public static readonly NavVector3Parser Instance = new NavVector3Parser();

        public Vector3 Parse(IReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            return new Vector3(x, y, z);
        }
    }
}
