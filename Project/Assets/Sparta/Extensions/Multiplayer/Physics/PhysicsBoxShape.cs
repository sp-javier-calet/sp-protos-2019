using System;
using System.Collections;
using BulletSharp;
using BulletSharp.Math;

namespace SocialPoint.Multiplayer
{
    //[AddComponentMenu("Physics Bullet/Shapes/Box")]
    public class PhysicsBoxShape : PhysicsCollisionShape
    {
        //[SerializeField]
        protected Vector3 extents = Vector3.One;

        //TODO: Set debugger
        PhysicsDebugger _debugger;

        public PhysicsDebugger Debugger
        {
            get
            {
                return _debugger;
            }
            set
            {
                _debugger = value;
            }
        }

        public NetworkGameObject GameObject
        {
            get;
            set;
        }

        public Vector3 Extents
        {
            get { return extents; }
            set
            {
                if(collisionShapePtr != null && value != extents)
                {
                    _debugger.LogError("Cannot change the extents after the bullet shape has been created. Extents is only the initial value " +
                    "Use LocalScaling to change the shape of a bullet shape.");
                }
                else
                {
                    extents = value;
                }
            }
        }

        //[SerializeField]
        protected Vector3 m_localScaling = Vector3.One;

        public Vector3 LocalScaling
        {
            get { return m_localScaling; }
            set
            {
                m_localScaling = value;
                if(collisionShapePtr != null)
                {
                    ((BoxShape)collisionShapePtr).LocalScaling = value;
                }
            }
        }

        /*public override void OnDrawGizmosSelected()
        {
            Vector3 position = GameObject.Transform.Position;
            Quaternion rotation = GameObject.Transform.Rotation;
            Vector3 scale = m_localScaling;
            //BUtility.DebugDrawBox(position, rotation, scale, extents, Color.yellow);
        }*/

        public override CollisionShape GetCollisionShape()
        {
            if(collisionShapePtr == null)
            {
                collisionShapePtr = new BoxShape(extents);
                ((BoxShape)collisionShapePtr).LocalScaling = m_localScaling;
            }
            return collisionShapePtr;
        }
    }
}
