using SharpNav;
using SocialPoint.IO;

namespace SocialPoint.Pathfinding
{
    public class NavPolyVertexSerializer : SimpleWriteSerializer<PolyVertex>
    {
        public static readonly NavPolyVertexSerializer Instance = new NavPolyVertexSerializer();

        public override void Serialize(PolyVertex value, IWriter writer)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
        }
    }

    public class NavPolyVertexParser : SimpleReadParser<PolyVertex>
    {
        public static readonly NavPolyVertexParser Instance = new NavPolyVertexParser();

        public override PolyVertex Parse(IReader reader)
        {
            int x = reader.ReadInt32();
            int y = reader.ReadInt32();
            int z = reader.ReadInt32();
            return new PolyVertex(x, y, z);
        }
    }
}
