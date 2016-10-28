using SocialPoint.Network;
using SocialPoint.Multiplayer;
using SocialPoint.IO;
using Jitter.LinearMath;

public class MovementAction : INetworkShareable
{
    public JVector Movement;

    public void Deserialize(IReader reader)
    {
        Movement = JVectorParser.Instance.Parse(reader);
    }

    public void Serialize(IWriter writer)
    {
        JVectorSerializer.Instance.Serialize(Movement, writer);
    }

    public static void Apply(NetworkScene scene, MovementAction action)
    {
        var go = scene.FindObject(1);
        if(go != null)
        {
            go.Transform.Position += action.Movement;
        }
    }
}