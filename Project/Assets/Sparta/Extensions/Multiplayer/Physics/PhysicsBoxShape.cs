using System;
using System.Collections;
using Jitter.LinearMath;
using Jitter.Collision.Shapes;

namespace SocialPoint.Multiplayer
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

        public JVector LocalScaling
        {
            get
            { 
                return _localScaling; 
            }
            set
            {
                _localScaling = value;
                //((BoxShape)_collisionShapePtr).LocalScaling = value;
            }
        }

        JVector _extents;

        JVector _localScaling;

        public PhysicsBoxShape() : this(JVector.One)
        {
        }

        public PhysicsBoxShape(JVector extents)
        {
            _extents = extents;
            _localScaling = JVector.One;

            _collisionShapePtr = new BoxShape(_extents);
            //((BoxShape)_collisionShapePtr).LocalScaling = _localScaling;
        }

        public override Object Clone()
        {
            return new PhysicsBoxShape(Extents);
        }
    }
}
