using SharpNav;
using SocialPoint.IO;

namespace SocialPoint.Pathfinding
{
    public class NavPolyBoundsSerializer : SimpleWriteSerializer<PolyBounds>
    {
        public static readonly NavPolyBoundsSerializer Instance = new NavPolyBoundsSerializer();

        public override void Serialize(PolyBounds value, IWriter writer)
        {
            NavPolyVertexSerializer.Instance.Serialize(value.Min, writer);
            NavPolyVertexSerializer.Instance.Serialize(value.Max, writer);
        }
    }

    public class NavPolyBoundsParser : SimpleReadParser<PolyBounds>
    {
        public static readonly NavPolyBoundsParser Instance = new NavPolyBoundsParser();

        public override PolyBounds Parse(IReader reader)
        {
            PolyVertex min = NavPolyVertexParser.Instance.Parse(reader);
            PolyVertex max = NavPolyVertexParser.Instance.Parse(reader);
            return new PolyBounds(min, max);
        }
    }
}
