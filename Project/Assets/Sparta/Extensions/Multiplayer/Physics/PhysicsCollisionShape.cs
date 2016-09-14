using System;
using System.Collections;
using BulletSharp.Math;
using BulletSharp;

namespace SocialPoint.Multiplayer
{
    [System.Serializable]
    public abstract class PhysicsCollisionShape : IDisposable, ICloneable
    {
        // Derived classes must create it upon construction
        protected CollisionShape _collisionShapePtr = null;

        public CollisionShape GetCollisionShape()
        {
            return _collisionShapePtr;
        }

        public void Dispose()
        {
            PhysicsUtilities.DisposeMember(ref _collisionShapePtr);
            GC.SuppressFinalize(this);
        }

        public abstract Object Clone();
    }
}