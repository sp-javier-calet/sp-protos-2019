using SocialPoint.Dependency;
using SocialPoint.Lifecycle;
using SocialPoint.Network;
using SocialPoint.Utils;

namespace Examples.Multiplayer.Lockstep
{
    public class ServerGameModeController : LifecycleController, IGameModeController
    {
        public virtual IEventProcessor Events { get { return null; } set {} }

        public GameNetworkSceneController SceneController { get; private set; }

        protected ServerGameModeController(IUpdateScheduler scheduler) : base(scheduler)
        {
        }

        public ServerGameModeController(IUpdateScheduler scheduler, ClientConfig config) : base(scheduler)
        {
            CreateServerComponents(scheduler, config);
        }

        protected void CreateServerComponents(IUpdateScheduler scheduler, ClientConfig config, GameNetworkSceneController sceneController = null)
        {
            var serverFactory = Services.Instance.Resolve<INetworkServerFactory>();
            var netServer = serverFactory.Create();

            var serverComp = new NetworkServerLifecycleComponent(netServer);
            RegisterComponent(serverComp);

            var lockstepComp = new LockstepNetworkServerLifecycleComponent(scheduler, serverComp.NetworkServer, config.General, sceneController);
            RegisterComponent(lockstepComp);

            SceneController = lockstepComp.SceneController;
            SceneController.Finished += Teardown;
        }
    }
}