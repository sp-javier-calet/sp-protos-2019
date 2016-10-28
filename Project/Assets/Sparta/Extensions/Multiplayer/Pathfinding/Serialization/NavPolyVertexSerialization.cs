using SharpNav;
using SocialPoint.IO;

namespace SocialPoint.Pathfinding
{
    public class NavPolyVertexSerializer : IWriteSerializer<PolyVertex>
    {
        public static readonly NavPolyVertexSerializer Instance = new NavPolyVertexSerializer();

        public void Serialize(PolyVertex value, IWriter writer)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
        }
    }

    public class NavPolyVertexParser : IReadParser<PolyVertex>
    {
        public static readonly NavPolyVertexParser Instance = new NavPolyVertexParser();

        public PolyVertex Parse(IReader reader)
        {
            int x = reader.ReadInt32();
            int y = reader.ReadInt32();
            int z = reader.ReadInt32();
            return new PolyVertex(x, y, z);
        }
    }
}
