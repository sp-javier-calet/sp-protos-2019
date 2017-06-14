using SocialPoint.Physics;

namespace SocialPoint.Multiplayer
{
    public class NetworkPhysicsWorld : PhysicsWorld, INetworkSceneBehaviour
    {
        public NetworkPhysicsWorld(bool multithreaded) : base(multithreaded)
        {
        }

        void INetworkSceneBehaviour.OnDestroy()
        {
        }

        void INetworkSceneBehaviour.Update(float dt)
        {
            base.Update(dt);
        }

        void INetworkSceneBehaviour.OnInstantiateObject(NetworkGameObject go)
        {
        }

        void INetworkSceneBehaviour.OnDestroyObject(int id)
        {
        }

        void INetworkSceneBehaviour.OnStart()
        {
        }

        NetworkScene INetworkSceneBehaviour.Scene
        {
            set
            {
            }
        }

    }
}
