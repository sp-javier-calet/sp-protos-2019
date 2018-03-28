using SocialPoint.Lifecycle;
using SocialPoint.Utils;

namespace Examples.Multiplayer.Lockstep
{
    public class HostGameModeController : ServerGameModeController
    {
        protected LockstepClientLifecycleComponent _clientComponent;

        public override IEventProcessor Events { get; set; }

        public HostGameModeController(IUpdateScheduler scheduler, ClientConfig config) : base(scheduler)
        {
            CreateComponents(scheduler, config);
        }

        protected virtual void CreateComponents(IUpdateScheduler scheduler, ClientConfig config)
        {
            _clientComponent = new LockstepClientLifecycleComponent(scheduler, config);
            
            CreateServerComponents(scheduler, config, _clientComponent.SceneController);
            RegisterComponent(new LockstepStoreReplayLifecycleComponent(_clientComponent.Lockstep, _clientComponent.SceneController.CommandFactory, config.ReplayPath));

            RegisterComponent(_clientComponent);

            RegisterClickValidation(config);
        }

        protected void RegisterClickValidation(ClientConfig config)
        {
            Events = new EventProcessor();

            Events.RegisterValidator(new ClickInputValidator(SceneController, config.General));
            Events.RegisterSuccessHandler(new ClickInputSuccessEventHandler(_clientComponent.Lockstep, config));
            Events.RegisterFailureHandler(new ClickInputFailureEventHandler());
        }
    }
}