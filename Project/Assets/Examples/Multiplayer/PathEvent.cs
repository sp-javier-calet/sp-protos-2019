using SocialPoint.Multiplayer;
using SocialPoint.IO;
using Jitter.LinearMath;

public class PathEvent : INetworkShareable
{
    public JVector[] Points;

    public void Deserialize(IReader reader)
    {
        Points = reader.ReadArray<JVector>(JVectorParser.Instance.Parse);
    }

    public void Serialize(IWriter writer)
    {
        writer.WriteArray<JVector>(Points, JVectorSerializer.Instance.Serialize);
    }
}