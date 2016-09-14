using System;
using System.Collections;
using BulletSharp;
using BulletSharp.Math;

namespace SocialPoint.Multiplayer
{
    public class PhysicsBoxShape : PhysicsCollisionShape
    {
        public Vector3 Extents
        {
            get
            {
                return _extents;
            }
        }

        public Vector3 LocalScaling
        {
            get
            { 
                return _localScaling; 
            }
            set
            {
                _localScaling = value;
                ((BoxShape)_collisionShapePtr).LocalScaling = value;
            }
        }

        Vector3 _extents;

        Vector3 _localScaling;

        public PhysicsBoxShape() : this(Vector3.One)
        {
        }

        public PhysicsBoxShape(Vector3 extents)
        {
            _extents = extents;
            _localScaling = Vector3.One;

            _collisionShapePtr = new BoxShape(_extents);
            ((BoxShape)_collisionShapePtr).LocalScaling = _localScaling;
        }

        public override Object Clone()
        {
            return new PhysicsBoxShape(Extents);
        }
    }
}
