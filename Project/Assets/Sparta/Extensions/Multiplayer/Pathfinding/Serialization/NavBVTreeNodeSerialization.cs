using SharpNav.Collections;
using SocialPoint.IO;

namespace SocialPoint.Pathfinding
{
    public class NavBVTreeNodeSerializer : SimpleWriteSerializer<BVTree.Node>
    {
        public static readonly NavBVTreeNodeSerializer Instance = new NavBVTreeNodeSerializer();

        public override void Serialize(BVTree.Node value, IWriter writer)
        {
            NavPolyBoundsSerializer.Instance.Serialize(value.Bounds, writer);
            writer.Write(value.Index);
        }
    }

    public class NavBVTreeNodeParser : SimpleReadParser<BVTree.Node>
    {
        public static readonly NavBVTreeNodeParser Instance = new NavBVTreeNodeParser();

        public override BVTree.Node Parse(IReader reader)
        {
            var node = new BVTree.Node();
            node.Bounds = NavPolyBoundsParser.Instance.Parse(reader);
            node.Index = reader.ReadInt32();
            return node;
        }
    }
}

