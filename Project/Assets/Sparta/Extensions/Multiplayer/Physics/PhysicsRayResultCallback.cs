using System.Collections;
using BulletSharp;
using BulletSharp.Math;

namespace SocialPoint.Multiplayer
{
    //TODO: Check the no-abstract classes that inherit from RayResultCallback (same file) to see if we can reuse or work with them
    public class PhysicsRayResultCallback : RayResultCallback
    {
        int _targetId = 0;
        bool hitObject = false;

        public PhysicsRayResultCallback(int targetId)
        {
            _targetId = targetId;
        }

        public override float AddSingleResult(LocalRayResult rayResult, bool normalInWorldSpace)
        {
            PhysicsCollisionObject co = (PhysicsCollisionObject)(rayResult.CollisionObject.UserObject);
            if(co != null)
            {
                UnityEngine.Debug.Log("*** TEST Raycast with object: " + co.GameObject.Id);
                if(co.GameObject.Id == _targetId)
                {
                    hitObject = true;
                }
            }
            else
            {
                UnityEngine.Debug.Log("*** TEST Raycast no hit ");
            }

            return ClosestHitFraction;
        }

        public bool IsHit()
        {
            return hitObject;
        }
    }
}
