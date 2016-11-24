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
