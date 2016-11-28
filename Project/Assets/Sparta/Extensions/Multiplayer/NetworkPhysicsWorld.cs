using SocialPoint.Physics;

namespace SocialPoint.Multiplayer
{
    public class NetworkPhysicsWorld : PhysicsWorld, INetworkServerSceneBehaviour
    {
        public NetworkPhysicsWorld(bool multithreaded) : base(multithreaded)
        {
        }

        public void Update(float dt, NetworkScene scene, NetworkScene oldScene)
        {
            base.Update(dt);
        }

        public void OnClientConnected(byte clientId)
        {
        }

        public void OnClientDisconnected(byte clientId)
        {
        }
    }
}