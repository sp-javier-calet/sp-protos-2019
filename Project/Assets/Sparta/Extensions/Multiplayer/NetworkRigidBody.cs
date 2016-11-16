﻿using SocialPoint.Physics;

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
            if(_rigidBody.EnableDebugDraw && _debugger != null)
            {
                _rigidBody.DebugDraw(_debugger);
            }
        }

        public void OnDestroy()
        {
            RemoveObjectFromPhysicsWorld();
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
