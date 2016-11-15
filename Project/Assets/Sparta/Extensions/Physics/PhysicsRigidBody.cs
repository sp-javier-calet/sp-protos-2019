using System;
using System.Collections;
using Jitter.LinearMath;
using Jitter.Dynamics;
using Jitter.Collision;
using Jitter.Collision.Shapes;

namespace SocialPoint.Physics
{
    public class PhysicsRigidBody
    {
        public enum ControlType
        {
            Dynamic,
            Kinematic,
            Static
        }

        public bool DoDebugDraw
        {
            get
            {
                return _rigidBody.EnableDebugDraw;
            }
            set
            {
                _rigidBody.EnableDebugDraw = value;
            }
        }

        protected RigidBody _rigidBody;
        protected PhysicsCollisionShape _collisionShape;
        protected ControlType _controlType;
        protected PhysicsWorld _physicsWorld;
        protected bool _isInWorld = false;
        protected IPhysicsDebugger _debugger;

        public int LayerIndex // 0-31 (int)
        {
            get
            {
                return _rigidBody.LayerIndex;
            }
            set
            {
                _rigidBody.LayerIndex = value;
            }
        }

        event CollisionDetectedHandler _collisionListeners;

        public PhysicsRigidBody(PhysicsCollisionShape shape, ControlType type, PhysicsWorld physicsWorld, IPhysicsDebugger debugger = null)
        {
            _collisionShape = shape;
            _controlType = type;
            _physicsWorld = physicsWorld;
            _debugger = debugger;

            BuildPhysicObject();
        }

        public Object Clone()
        {
            PhysicsCollisionShape shapeClone = (PhysicsCollisionShape)_collisionShape.Clone();
            var behavior = new PhysicsRigidBody(shapeClone, _controlType, _physicsWorld, _debugger);
            return behavior;
        }

        public void OnCollision(RigidBody other, JVector myPoint, JVector otherPoint, JVector normal, float penetration)
        {
            if(_collisionListeners != null)
            {
                _collisionListeners(_rigidBody, other, myPoint, otherPoint, normal, penetration);
            }
        }

        public void AddCollisionHandler(CollisionDetectedHandler handler)
        {
            _collisionListeners += handler;
        }

        public void RemoveCollisionHandler(CollisionDetectedHandler handler)
        {
            _collisionListeners -= handler;
        }

        public void AddImpulse(JVector impulse)
        {
            if(_isInWorld)
            {
                _rigidBody.ApplyImpulse(impulse);
            }
        }

        public void AddImpulseAtPosition(JVector impulse, JVector relativePostion)
        {
            if(_isInWorld)
            {
                _rigidBody.ApplyImpulse(impulse, relativePostion);
            }
        }

        public void AddForce(JVector force)
        {
            if(_isInWorld)
            {
                _rigidBody.AddForce(force);
            }
        }

        public void AddForceAtPosition(JVector force, JVector relativePostion)
        {
            if(_isInWorld)
            {
                _rigidBody.AddForce(force, relativePostion);
            }
        }

        public void AddTorque(JVector torque)
        {
            if(_isInWorld)
            {
                _rigidBody.AddTorque(torque);
            }
        }

        protected void AddObjectToPhysicsWorld()
        {
            if(!_isInWorld)
            {
                _physicsWorld.AddRigidBody(_rigidBody);
                _isInWorld = true;
            }
        }

        protected void RemoveObjectFromPhysicsWorld()
        {
            if(_isInWorld)
            {
                _physicsWorld.RemoveRigidBody(_rigidBody);
                _isInWorld = false;
            }
        }

        void BuildPhysicObject()
        {
            Shape cs = _collisionShape.GetCollisionShape();
            _rigidBody = new RigidBody(cs);
            _rigidBody.Tag = this;

            switch(_controlType)
            {
            case ControlType.Kinematic:
                _rigidBody.IsKinematic = true;
                break;
            case ControlType.Static:
                _rigidBody.IsStatic = true;
                break;
            default:
                break;
            }
        }
    }
}
