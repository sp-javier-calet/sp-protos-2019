using System.Collections;
using SocialPoint.Multiplayer;

public class MovementActionDelegate : INetworkActionDelegate
{
    public void ApplyAction(object action, NetworkScene scene)
    {
        MovementAction movementAction = (MovementAction)action;
        var itr = scene.GetObjectEnumerator();
        var go = scene.FindObject(1);
        go.Transform.Position += movementAction.Movement;
        itr.Dispose();
    }
}
