using System.Collections;
using BulletSharp;
using BulletSharp.Math;

namespace SocialPoint.Multiplayer
{
    //TODO: Check the no-abstract classes that inherit from RayResultCallback (same file) to see if we can reuse or work with them
    public class NetworkGameObjectRayResultCallback : RayResultCallback
    {
        public NetworkGameObject GameObjectHit
        {
            get;
            private set;
        }

        public override float AddSingleResult(LocalRayResult rayResult, bool normalInWorldSpace)
        {
            PhysicsCollisionObject co = (PhysicsCollisionObject)(rayResult.CollisionObject.UserObject);
            if(co != null)
            {
                GameObjectHit = co.NetworkGameObject;
            }
            return ClosestHitFraction;
        }
    }
}
