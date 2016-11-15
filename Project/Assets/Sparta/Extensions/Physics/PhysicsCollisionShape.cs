using System;
using System.Collections;
using Jitter.Collision.Shapes;

namespace SocialPoint.Physics
{
    [System.Serializable]
    public abstract class PhysicsCollisionShape : ICloneable
    {
        // Derived classes must create it upon construction
        protected Shape _collisionShapePtr = null;

        public Shape GetCollisionShape()
        {
            return _collisionShapePtr;
        }

        public abstract Object Clone();
    }
}