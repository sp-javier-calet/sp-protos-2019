using System.Collections;
using SocialPoint.Multiplayer;

public class MovementActionDelegate : INetworkActionDelegate
{
    public void ApplyAction(object action, NetworkScene scene)
    {
        MovementAction movementAction = (MovementAction)action;
        var itr = scene.GetObjectEnumerator();
        while(itr.MoveNext())
        {
            var go = itr.Current;
            go.Transform.Position += movementAction.Movement;
            go.Transform.Size *= 1;
        }
        itr.Dispose();
    }
}
