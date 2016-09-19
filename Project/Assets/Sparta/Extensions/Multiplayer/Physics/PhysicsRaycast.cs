using System.Collections;
using System.Collections.Generic;
using Jitter.LinearMath;

namespace SocialPoint.Multiplayer
{
    public class PhysicsRaycast
    {
        /// <summary>
        /// Raycast result with closest object hit.
        /// </summary>
        public class ClosestResult //: ClosestRayResultCallback
        {
            public NetworkGameObject GameObjectHit
            {
                get;
                private set;
            }

            public ClosestResult() : this(JVector.Zero, JVector.Zero)
            {
            }

            private ClosestResult(JVector from, JVector to)// : base(ref from, ref to)
            {
            }

            /*public override float AddSingleResult(LocalRayResult rayResult, bool normalInWorldSpace)
            {
                PhysicsCollisionObject co = (PhysicsCollisionObject)(rayResult.CollisionObject.UserObject);
                if(co != null)
                {
                    GameObjectHit = co.NetworkGameObject;
                }
                return base.AddSingleResult(rayResult, normalInWorldSpace);
            }*/
        }

        /// <summary>
        /// Raycast result with all objects hit.
        /// WARNING: Objects may be in any order.
        /// </summary>
        public class AllHitsResult //: AllHitsRayResultCallback
        {
            public List<NetworkGameObject> GameObjectsHit
            {
                get;
                private set;
            }

            public AllHitsResult() : this(JVector.Zero, JVector.Zero)
            {
            }

            private AllHitsResult(JVector from, JVector to)// : base(from, to)
            {
                GameObjectsHit = new List<NetworkGameObject>();
            }

            /*public override float AddSingleResult(LocalRayResult rayResult, bool normalInWorldSpace)
            {
                PhysicsCollisionObject co = (PhysicsCollisionObject)(rayResult.CollisionObject.UserObject);
                if(co != null)
                {
                    GameObjectsHit.Add(co.NetworkGameObject);
                }
                return base.AddSingleResult(rayResult, normalInWorldSpace);
            }*/
        }

        /* Raycast Calls */

        public static bool Raycast(Ray ray, PhysicsWorld physicsWorld, out ClosestResult rayResult)
        {
            return Raycast(ray, float.MaxValue, physicsWorld, out rayResult);
        }

        public static bool Raycast(Ray ray, float maxDistance, PhysicsWorld physicsWorld, out ClosestResult rayResult)
        {
            JVector startPoint;
            JVector endPoint;
            RaycastPointsFromRay(ref ray, maxDistance, out startPoint, out endPoint);

            rayResult = new ClosestResult();
            //rayResult.RayFromWorld = startPoint;
            //rayResult.RayToWorld = endPoint;

            return Raycast(ref startPoint, ref endPoint, physicsWorld, rayResult);
        }

        public static bool Raycast(Ray ray, PhysicsWorld physicsWorld, out AllHitsResult rayResult)
        {
            return Raycast(ray, float.MaxValue, physicsWorld, out rayResult);
        }

        public static bool Raycast(Ray ray, float maxDistance, PhysicsWorld physicsWorld, out AllHitsResult rayResult)
        {
            JVector startPoint;
            JVector endPoint;
            RaycastPointsFromRay(ref ray, maxDistance, out startPoint, out endPoint);

            rayResult = new AllHitsResult();
            //rayResult.RayFromWorld = startPoint;
            //rayResult.RayToWorld = endPoint;

            return Raycast(ref startPoint, ref endPoint, physicsWorld, rayResult);
        }

        private static bool Raycast(ref JVector startPoint, ref JVector endPoint, PhysicsWorld physicsWorld, object rayResult)//RayResultCallback rayResult
        {
            //physicsWorld.CollisionWorld.RayTestRef(ref startPoint, ref endPoint, rayResult);
            return false;//rayResult.HasHit;
        }

        private static void RaycastPointsFromRay(ref Ray ray, float maxDistance, out JVector startPoint, out JVector endPoint)
        {
            startPoint = ray.origin;
            endPoint = startPoint + (ray.direction * maxDistance);
        }
    }
}