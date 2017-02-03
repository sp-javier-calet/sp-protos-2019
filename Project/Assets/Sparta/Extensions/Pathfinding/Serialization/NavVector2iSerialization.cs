using SharpNav.Geometry;
using SocialPoint.IO;

namespace SocialPoint.Pathfinding
{
    public class NavVector2iSerializer : IWriteSerializer<Vector2i>
    {
        public static readonly NavVector2iSerializer Instance = new NavVector2iSerializer();

        public void Serialize(Vector2i value, IWriter writer)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
        }
    }

    public class NavVector2iParser : IReadParser<Vector2i>
    {
        public static readonly NavVector2iParser Instance = new NavVector2iParser();

        public Vector2i Parse(IReader reader)
        {
            int x = reader.ReadInt32();
            int y = reader.ReadInt32();
            return new Vector2i(x, y);
        }
    }
}
