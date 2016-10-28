using SharpNav;
using SocialPoint.IO;

namespace SocialPoint.Pathfinding
{
    public class NavPolyBoundsSerializer : IWriteSerializer<PolyBounds>
    {
        public static readonly NavPolyBoundsSerializer Instance = new NavPolyBoundsSerializer();

        public void Serialize(PolyBounds value, IWriter writer)
        {
            NavPolyVertexSerializer.Instance.Serialize(value.Min, writer);
            NavPolyVertexSerializer.Instance.Serialize(value.Max, writer);
        }
    }

    public class NavPolyBoundsParser : IReadParser<PolyBounds>
    {
        public static readonly NavPolyBoundsParser Instance = new NavPolyBoundsParser();

        public PolyBounds Parse(IReader reader)
        {
            PolyVertex min = NavPolyVertexParser.Instance.Parse(reader);
            PolyVertex max = NavPolyVertexParser.Instance.Parse(reader);
            return new PolyBounds(min, max);
        }
    }
}
