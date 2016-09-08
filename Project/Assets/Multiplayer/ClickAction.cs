using SocialPoint.Network;
using SocialPoint.Multiplayer;
using SocialPoint.IO;
using BulletSharp.Math;

public class ClickAction : INetworkShareable
{
    public Vector3 Position;
    public Ray Ray;

    public void Deserialize(IReader reader)
    {
        Position = Vector3Parser.Instance.Parse(reader);
        Ray = RayParser.Instance.Parse(reader);
    }

    public void Serialize(IWriter writer)
    {
        Vector3Serializer.Instance.Serialize(Position, writer);
        RaySerializer.Instance.Serialize(Ray, writer);
    }
}