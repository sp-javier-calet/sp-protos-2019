using System;
using SocialPoint.Physics;

namespace SocialPoint.Multiplayer
{
    public class NetworkRigidBody : PhysicsRigidBody, INetworkBehaviour
    {

        public NetworkGameObject NetworkGameObject
        {
            get;
            private set;
        }

        public NetworkRigidBody(PhysicsCollisionShape shape, ControlType type, PhysicsWorld physicsWorld, IPhysicsDebugger debugger = null)
            : base(shape, type, physicsWorld, debugger)
        {
        }

        void INetworkBehaviour.OnStart(NetworkGameObject go)
        {
            NetworkGameObject = go;
            UpdateTransformFromGameObject();

            AddObjectToPhysicsWorld();
        }

        void INetworkBehaviour.Update(float dt)
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
            if(_rigidBody.EnableDebugDraw && _debugger != null)
            {
                _rigidBody.DebugDraw(_debugger);
            }
        }

        void INetworkBehaviour.OnDestroy()
        {
            RemoveObjectFromPhysicsWorld();
        }

        public object Clone()
        {
            PhysicsCollisionShape shapeClone = (PhysicsCollisionShape)_collisionShape.Clone();
            var behavior = new NetworkRigidBody(shapeClone, _controlType, _physicsWorld, _debugger);
            return behavior;
        }

        void UpdateTransformFromGameObject()
        {
            //Reactivate object if moving it
            if(!_rigidBody.IsActive)
            {
                bool moved = (_rigidBody.Position != NetworkGameObject.Transform.Position);
                if(moved)
                {
                    _rigidBody.IsActive = true;
                }
            }

            _rigidBody.Position = NetworkGameObject.Transform.Position;
        }

        void UpdateTransformFromPhysicsObject()
        {
            NetworkGameObject.Transform.Position = _rigidBody.Position;
        }
    }
}
