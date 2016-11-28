using SocialPoint.Network;
using SocialPoint.Multiplayer;
using SocialPoint.Physics;
using SocialPoint.IO;
using Jitter.LinearMath;

public class ClickAction : INetworkShareable
{
    public JVector Position;
    public Ray Ray;

    public void Deserialize(IReader reader)
    {
        Position = JVectorParser.Instance.Parse(reader);
        Ray = RayParser.Instance.Parse(reader);
    }

    public void Serialize(IWriter writer)
    {
        JVectorSerializer.Instance.Serialize(Position, writer);
        RaySerializer.Instance.Serialize(Ray, writer);
    }
}