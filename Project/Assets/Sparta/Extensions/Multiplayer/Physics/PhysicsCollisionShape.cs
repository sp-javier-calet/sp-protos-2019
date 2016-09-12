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

        // Derived classes must create it upon construction
        protected CollisionShape _collisionShapePtr = null;

        public CollisionShape GetCollisionShape()
        {
            return _collisionShapePtr;
        }

        public void Dispose()
        {
            PhysicsUtilities.Dispose(ref _collisionShapePtr);
            GC.SuppressFinalize(this);
        }
    }
}