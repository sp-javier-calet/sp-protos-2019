using SocialPoint.Dependency;
using SocialPoint.Lifecycle;
using SocialPoint.Network;
using SocialPoint.Utils;

namespace Examples.Multiplayer.Lockstep
{
    public class BaseClientGameModeController : LifecycleController, IGameModeController
    {
        readonly protected LockstepClientLifecycleComponent _clientComponent;

        public virtual IEventProcessor Events { get; private set; }

        public GameNetworkSceneController SceneController { get { return _clientComponent.SceneController; } }

        public BaseClientGameModeController(IUpdateScheduler scheduler, ClientConfig config) : base(scheduler)
        {
            _clientComponent = new LockstepClientLifecycleComponent(scheduler, config);
            _clientComponent.SceneController.Finished += Teardown;

            RegisterComponent(_clientComponent);
        }

        protected void RegisterClickValidation(ClientConfig config)
        {
            Events = new EventProcessor();

            Events.RegisterValidator(new ClickInputValidator(SceneController, config.General));
            Events.RegisterSuccessHandler(new ClickInputSuccessEventHandler(_clientComponent.Lockstep, config));
            Events.RegisterFailureHandler(new ClickInputFailureEventHandler());
        }
    }

    public class ClientGameModeController : BaseClientGameModeController
    {
        public ClientGameModeController(IUpdateScheduler scheduler, ClientConfig config) : base(scheduler, config)
        {
            var clientFactory = Services.Instance.Resolve<INetworkClientFactory>();
            var netClient = clientFactory.Create();

            var netClientComponent = new NetworkClientLifecycleComponent(netClient);
            RegisterComponent(netClientComponent);

            RegisterComponent(new LockstepNetworkClientLifecycleComponent(_clientComponent.SceneController, netClientComponent.NetworkClient));

            RegisterComponent(new LockstepStoreReplayLifecycleComponent(_clientComponent.Lockstep, _clientComponent.SceneController.CommandFactory, config.ReplayPath));

            RegisterClickValidation(config);
        }
    }
}