using System;
using System.Collections;
using BulletSharp.Math;
using BulletSharp;

namespace SocialPoint.Multiplayer
{
    [System.Serializable]
    public abstract class PhysicsCollisionShape : IDisposable
    {
        public enum CollisionShapeType
        {
            // dynamic
            BoxShape = 0,
            SphereShape = 1,
            CapsuleShape = 2,
            CylinderShape = 3,
            ConeShape = 4,
            ConvexHull = 5,
            CompoundShape = 6,

            // static
            BvhTriangleMeshShape = 7,
            StaticPlaneShape = 8,
        };

        protected CollisionShape collisionShapePtr = null;

        void OnDestroy()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isdisposing)
        {
            if(collisionShapePtr != null)
            {
                collisionShapePtr.Dispose();
                collisionShapePtr = null;
            }
        }

        //public abstract void OnDrawGizmosSelected();

        public abstract CollisionShape GetCollisionShape();
    }
}