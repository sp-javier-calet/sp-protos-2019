using System.Collections;
using SocialPoint.Multiplayer;
using BulletSharp;

public class DemoCollisionCallbackListener : PhysicsDefaultCollisionCallbacks
{
    PhysicsDebugger _debugger;

    public DemoCollisionCallbackListener(CollisionObject collisionObject, PhysicsDebugger debugger) : base(collisionObject)
    {
        _debugger = debugger;
    }

    public override void OnCollisionEnter(CollisionObject other, PersistentManifoldList manifoldList)
    {
        _debugger.Log(GetNetworkGameObject(_collisionObject).Id + " OnCollisionEnter with " + GetNetworkGameObject(other).Id);
    }

    public override void OnCollisionStay(CollisionObject other, PersistentManifoldList manifoldList)
    {
        _debugger.Log(GetNetworkGameObject(_collisionObject).Id + " OnCollisionStay with " + GetNetworkGameObject(other).Id);
    }

    public override void OnCollisionExit(CollisionObject other)
    {
        _debugger.Log(GetNetworkGameObject(_collisionObject).Id + " OnCollisionExit with " + GetNetworkGameObject(other).Id);
    }

    NetworkGameObject GetNetworkGameObject(CollisionObject co)
    {
        if(co.UserObject is PhysicsCollisionObject)
        {
            return ((PhysicsCollisionObject)co.UserObject).NetworkGameObject;
        }
        return null;
    }
}
