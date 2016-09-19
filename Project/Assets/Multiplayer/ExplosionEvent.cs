using SocialPoint.Multiplayer;
using SocialPoint.Network;
using SocialPoint.IO;
using Jitter.LinearMath;

public class ExplosionEvent : INetworkShareable
{
    public JVector Position;

    public void Deserialize(IReader reader)
    {
        Position = Vector3Parser.Instance.Parse(reader);
    }

    public void Serialize(IWriter writer)
    {
        Vector3Serializer.Instance.Serialize(Position, writer);
    }
}