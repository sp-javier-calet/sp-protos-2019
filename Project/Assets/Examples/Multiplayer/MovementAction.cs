using SocialPoint.Network;
using SocialPoint.Multiplayer;
using SocialPoint.IO;
using Jitter.LinearMath;

public class MovementAction : INetworkShareable
{
    public JVector Movement;

    public void Deserialize(IReader reader)
    {
        Movement = Vector3Parser.Instance.Parse(reader);
    }

    public void Serialize(IWriter writer)
    {
        Vector3Serializer.Instance.Serialize(Movement, writer);
    }

    public static void Apply(MovementAction action, NetworkScene scene)
    {
        var go = scene.FindObject(1);
        if(go != null)
        {
            go.Transform.Position += action.Movement;
        }
    }
}