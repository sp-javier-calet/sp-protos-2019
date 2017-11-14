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

        void INetworkSceneBehaviour.OnInstantiateObject(NetworkGameObject go)
        {
        }

        void INetworkSceneBehaviour.OnDestroyObject(int id)
        {
        }

        void INetworkSceneBehaviour.OnStart()
        {
        }

        public void Dispose()
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
