using System;
using System.Collections;
using Jitter.Collision.Shapes;

namespace SocialPoint.Multiplayer
{
    [System.Serializable]
    public abstract class PhysicsCollisionShape : IDisposable, ICloneable
    {
        // Derived classes must create it upon construction
        protected Shape _collisionShapePtr = null;

        public Shape GetCollisionShape()
        {
            return _collisionShapePtr;
        }

        public void Dispose()
        {
            //PhysicsUtilities.DisposeMember(ref _collisionShapePtr);
            GC.SuppressFinalize(this);
        }

        public abstract Object Clone();
    }
}