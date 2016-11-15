using System;
using System.Collections;
using Jitter.LinearMath;
using Jitter.Collision.Shapes;

namespace SocialPoint.Physics
{
    public class PhysicsBoxShape : PhysicsCollisionShape
    {
        public JVector Extents
        {
            get
            {
                return _extents;
            }
        }

        JVector _extents;

        public PhysicsBoxShape() : this(JVector.One)
        {
        }

        public PhysicsBoxShape(JVector extents)
        {
            _extents = extents;

            _collisionShapePtr = new BoxShape(_extents);
        }

        public override Object Clone()
        {
            return new PhysicsBoxShape(Extents);
        }
    }
}
