using SocialPoint.IO;
using SocialPoint.Multiplayer;
using SocialPoint.Network;
using SocialPoint.Physics;
using Jitter.LinearMath;

public class ExplosionEvent : INetworkShareable
{
    public JVector Position;

    public void Deserialize(IReader reader)
    {
        Position = JVectorParser.Instance.Parse(reader);
    }

    public void Serialize(IWriter writer)
    {
        JVectorSerializer.Instance.Serialize(Position, writer);
    }
}