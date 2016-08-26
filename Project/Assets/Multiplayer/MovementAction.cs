using SocialPoint.Network;
using SocialPoint.Multiplayer;
using SocialPoint.IO;

public class MovementAction : INetworkShareable
{
    public Vector3 Movement;

    public void Deserialize(IReader reader)
    {
        Movement = Vector3Parser.Instance.Parse(reader);
    }

    public void Serialize(IWriter writer)
    {
        Vector3Serializer.Instance.Serialize(Movement, writer);
    }
}