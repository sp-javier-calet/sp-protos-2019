using System.Collections;
using System.Collections.Generic;
using Jitter;
using Jitter.Collision;
using Jitter.Dynamics;
using Jitter.LinearMath;

namespace SocialPoint.Multiplayer
{
    public static class PhysicsRaycast
    {
        public struct Result
        {
            public PhysicsRigidBody ObjectHit;
            public JVector HitNormal;
            public float Fraction;
        }

        /* Raycast Calls */

        public static bool Raycast(Ray ray, PhysicsWorld physicsWorld, out Result rayResult)
        {
            return Raycast(ray, float.MaxValue, physicsWorld, out rayResult);
        }

        public static bool Raycast(Ray ray, float maxDistance, PhysicsWorld physicsWorld, out Result rayResult)
        {
            JVector startPoint;
            JVector endPoint;
            RaycastPointsFromRay(ref ray, maxDistance, out startPoint, out endPoint);

            return Raycast(ref startPoint, ref endPoint, physicsWorld, out rayResult, ray.LayerIndex);
        }

        static bool Raycast(ref JVector startPoint, ref JVector endPoint, PhysicsWorld physicsWorld, out Result rayResult, int rayLayerIdx)
        {
            rayResult = new Result();
            RigidBody resBody;
            JVector hitNormal;
            float fraction;

            if(physicsWorld.World.CollisionSystem.Raycast(startPoint, endPoint, null, out resBody, out hitNormal, out fraction))
            {
                
                var objectHit = (PhysicsRigidBody)resBody.Tag;
                if(!physicsWorld.IsCollisionEnabled(objectHit.LayerIndex, rayLayerIdx))
                {
                    return false;
                }

                rayResult.ObjectHit = objectHit;
                rayResult.HitNormal = hitNormal;
                rayResult.Fraction = fraction;

            }
            return false;
        }

        static void RaycastPointsFromRay(ref Ray ray, float maxDistance, out JVector startPoint, out JVector endPoint)
        {
            startPoint = ray.Origin;
            endPoint = startPoint + (ray.Direction * maxDistance);
        }
    }
}