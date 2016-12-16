using SocialPoint.Geometry;
using SocialPoint.Multiplayer;
using SocialPoint.IO;
using Jitter.LinearMath;

public class MovementAction : INetworkShareable
{
    public JVector Movement;

    public void Deserialize(IReader reader)
    {
        Movement = VectorParser.Instance.Parse(reader);
    }

    public void Serialize(IWriter writer)
    {
        VectorSerializer.Instance.Serialize(Movement, writer);
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