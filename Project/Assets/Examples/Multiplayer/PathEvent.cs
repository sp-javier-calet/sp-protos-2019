using SocialPoint.Geometry;
using SocialPoint.IO;
using Jitter.LinearMath;

public class PathEvent : INetworkShareable
{
    public Vector[] Points;

    public void Deserialize(IReader reader)
    {
        Points = reader.ReadArray<Vector>(VectorParser.Instance.Parse);
    }

    public void Serialize(IWriter writer)
    {
        writer.WriteArray<Vector>(Points, VectorSerializer.Instance.Serialize);
    }
}