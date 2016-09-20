using System;
using System.Collections;
using Jitter.LinearMath;
using Jitter.Dynamics;
using Jitter.Collision;
using Jitter.Collision.Shapes;

namespace SocialPoint.Multiplayer
{
    public class PhysicsRigidBody : INetworkBehaviour
    {
        public enum ControlType
        {
            Dynamic,
            Kinematic,
            Static
        }

        public NetworkGameObject NetworkGameObject
        {
            get;
            private set;
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

        RigidBody _rigidBody;
        PhysicsCollisionShape _collisionShape;
        ControlType _controlType;
        IPhysicsDebugger _debugger;
        PhysicsWorld _physicsWorld;
        bool _isInWorld = false;

        event CollisionDetectedHandler _collisionListeners;

        public PhysicsRigidBody(PhysicsCollisionShape shape, ControlType type, PhysicsWorld physicsWorld, IPhysicsDebugger debugger)
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

        public void OnStart(NetworkGameObject go)
        {
            NetworkGameObject = go;
            UpdateTransformFromGameObject();

            AddObjectToPhysicsWorld();
        }

        public void Update(float dt)
        {
            //Update object transform
            switch(_controlType)
            {
            case ControlType.Kinematic:
                UpdateTransformFromGameObject();
                break;
            case ControlType.Dynamic:
                UpdateTransformFromPhysicsObject();
                break;
            default:
                break;
            }

            //Debug if requested
            if(_rigidBody.EnableDebugDraw)
            {
                _rigidBody.DebugDraw(_debugger);
            }
        }

        public void OnDestroy()
        {
            RemoveObjectFromPhysicsWorld();
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

        void UpdateTransformFromGameObject()
        {
            _rigidBody.Position = NetworkGameObject.Transform.Position;
        }

        void UpdateTransformFromPhysicsObject()
        {
            NetworkGameObject.Transform.Position = _rigidBody.Position;
        }

        void AddObjectToPhysicsWorld()
        {
            if(!_isInWorld)
            {
                _physicsWorld.AddRigidBody(_rigidBody);
                _isInWorld = true;
            }
        }

        void RemoveObjectFromPhysicsWorld()
        {
            if(_isInWorld)
            {
                _physicsWorld.RemoveRigidBody(_rigidBody);
                _isInWorld = false;
            }
        }
    }
}
