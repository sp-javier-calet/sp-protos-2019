using SocialPoint.Geometry;
using SocialPoint.Physics;
using SocialPoint.IO;
using Jitter.LinearMath;

public class ClickAction : INetworkShareable
{
    public JVector Position;
    public Ray Ray;

    public void Deserialize(IReader reader)
    {
        Position = VectorParser.Instance.Parse(reader);
        Ray = RayParser.Instance.Parse(reader);
    }

    public void Serialize(IWriter writer)
    {
        VectorSerializer.Instance.Serialize(Position, writer);
        RaySerializer.Instance.Serialize(Ray, writer);
    }
}