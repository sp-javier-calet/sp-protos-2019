using Jitter.LinearMath;
using SocialPoint.Physics;
using SocialPoint.Utils;

namespace SocialPoint.Multiplayer
{
    public class NetworkRigidBody : PhysicsRigidBody, INetworkBehaviour, ILateUpdateable
    {
        public static bool EnableRigidBody = true;

        NetworkGameObject _go;

        public NetworkGameObject GameObject
        {
            get
            {
                return _go;
            }
            set
            {
                _go = value;
            }
        }

        public NetworkRigidBody Init(IPhysicsShape shape, ControlType type, PhysicsWorld physicsWorld)
        {
            base.Init(shape, type, physicsWorld);
            return this;
        }

        public void OnAwake()
        {
            AddObjectToPhysicsWorld();
        }

        public void OnStart()
        {
            UpdateTransformFromGameObject();
        }

        public void Update(float dt)
        {
            if(!EnableRigidBody)
            {
                return;
            }

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
        }

        public void LateUpdate(float dt)
        {
        }

        public void OnDestroy()
        {
            RemoveObjectFromPhysicsWorld();
        }

        public object Clone()
        {
            var shapeClone = (IPhysicsShape)_shape.Clone();
            var behaviour = _go != null && _go.Context != null ? _go.Context.Pool.Get<NetworkRigidBody>() : new NetworkRigidBody();
            behaviour.Init(shapeClone, _controlType, _physicsWorld);
            behaviour.GameObject = _go;
            return behaviour;
        }

        public void Dispose()
        {
            if(_go != null)
            {
                _go.Context.Pool.Return(this);
            }
        }

        void UpdateTransformFromGameObject()
        {
            if(_go == null)
            {
                return;
            }
            var newPos = _go.Transform.Position;

            var newRot = JMatrix.CreateFromQuaternion(_go.Transform.Rotation);

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
            _go.Transform.Position = _rigidBody.Position;
            _go.Transform.Rotation = JQuaternion.CreateFromMatrix(_rigidBody.Orientation);
        }
    }
}
