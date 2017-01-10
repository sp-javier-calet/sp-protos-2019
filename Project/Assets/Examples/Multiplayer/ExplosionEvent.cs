using SocialPoint.Geometry;
using SocialPoint.IO;
using Jitter.LinearMath;

public class ExplosionEvent : INetworkShareable
{
    public JVector Position;

    public void Deserialize(IReader reader)
    {
        Position = VectorParser.Instance.Parse(reader);
    }

    public void Serialize(IWriter writer)
    {
        VectorSerializer.Instance.Serialize(Position, writer);
    }
}