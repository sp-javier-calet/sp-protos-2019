using System;
using System.Collections.Generic;
using Jitter;
using Jitter.LinearMath;
using Jitter.Dynamics;

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

        public bool EnableDebugDraw
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
        protected IPhysicsShape _shape;
        protected ControlType _controlType;
        protected PhysicsWorld _physicsWorld;
        protected bool _isInWorld = false;

        protected IPhysicsDebugger _debugger;

        public RigidBody RigidBody
        {
            get
            {
                return _rigidBody; 
            }
        }

        public JVector Position
        {
            get
            {
                return _rigidBody.Position;
            }
        }

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

        public delegate void CollisionHandler(RigidBody other, List<PhysicsContact> ContactSettings);

        event CollisionHandler _collisionStayListeners;

        event CollisionHandler _collisionEnterListeners;

        event CollisionHandler _collisionExitListeners;

        public PhysicsRigidBody Init(IPhysicsShape shape, ControlType type, PhysicsWorld physicsWorld, IPhysicsDebugger debugger = null)
        {
            if(shape == null)
            {
                throw new ArgumentNullException("shape");
            }
            if(physicsWorld == null)
            {
                throw new ArgumentNullException("physicsWorld");
            }
            _shape = shape;
            _controlType = type;
            _physicsWorld = physicsWorld;
            _debugger = debugger;
            return this;
        }

        public void DebugDraw(IDebugDrawer drawer)
        {
            _rigidBody.DebugDraw(drawer);
        }

        public void OnCollisionStay(RigidBody other, List<PhysicsContact> contacts)
        {
            if(_collisionStayListeners != null)
            {
                _collisionStayListeners(other, contacts);
            }
        }

        public void OnCollisionEnter(RigidBody other, List<PhysicsContact> contacts)
        {
            if(_collisionEnterListeners != null)
            {
                _collisionEnterListeners(other, contacts);
            }
        }

        public void OnCollisionExit(RigidBody other, List<PhysicsContact> contacts)
        {
            if(_collisionExitListeners != null)
            {
                _collisionExitListeners(other, contacts);
            }
        }

        public void AddCollisionStayHandler(CollisionHandler handler)
        {
            _collisionStayListeners += handler;
        }

        public void RemoveCollisionStayHandler(CollisionHandler handler)
        {
            _collisionStayListeners -= handler;
        }

        public void AddCollisionEnterHandler(CollisionHandler handler)
        {
            _collisionEnterListeners += handler;
        }

        public void RemoveCollisionEnterHandler(CollisionHandler handler)
        {
            _collisionEnterListeners -= handler;
        }

        public void AddCollisionExitHandler(CollisionHandler handler)
        {
            _collisionExitListeners += handler;
        }

        public void RemoveCollisionExitHandler(CollisionHandler handler)
        {
            _collisionExitListeners -= handler;
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

        public void AddObjectToPhysicsWorld()
        {
            if(!_isInWorld)
            {
                BuildPhysicObject();
                _physicsWorld.AddRigidBody(_rigidBody);
                _isInWorld = true;
            }
        }

        public void RemoveObjectFromPhysicsWorld()
        {
            if(_isInWorld)
            {
                _physicsWorld.RemoveRigidBody(_rigidBody);
                _isInWorld = false;
            }
        }

        void BuildPhysicObject()
        {
            _rigidBody = new RigidBody(_shape.CollisionShape);
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
