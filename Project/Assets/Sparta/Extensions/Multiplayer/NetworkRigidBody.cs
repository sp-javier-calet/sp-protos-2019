using System;
using SocialPoint.Physics;
using Jitter.LinearMath;

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
            var newPos = NetworkGameObject.Transform.Position;
            var newRot = JMatrix.CreateFromQuaternion(NetworkGameObject.Transform.Rotation);

            bool moved = (_rigidBody.Position != newPos);
            bool rotated = moved ? true : (_rigidBody.Orientation != newRot);//If moved, we can avoid to check if rotated

            if(moved || rotated)
            {
                _rigidBody.UpdateTransform(ref newPos, ref newRot);

                //Reactivate object if moving it
                if(!_rigidBody.IsActive)
                {
                    _rigidBody.IsActive = true;
                }
            }
        }

        void UpdateTransformFromPhysicsObject()
        {
            NetworkGameObject.Transform.Position = _rigidBody.Position;
            NetworkGameObject.Transform.Rotation = JQuaternion.CreateFromMatrix(_rigidBody.Orientation);
        }
    }
}
