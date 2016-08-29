using System.Collections;
using SocialPoint.Multiplayer;

public class MovementActionDelegate : INetworkActionDelegate
{
    public void ApplyAction(object action, NetworkScene scene)
    {
        var go = scene.FindObject(1);
        if(go != null)
        {
            MovementAction movementAction = (MovementAction)action;
            go.Transform.Position += movementAction.Movement;
        }
    }
}
