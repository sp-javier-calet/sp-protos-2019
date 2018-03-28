using SocialPoint.Utils;

namespace Examples.Multiplayer.Lockstep
{
    public class LocalGameModeController : BaseClientGameModeController
    {
        public LocalGameModeController(IUpdateScheduler scheduler, ClientConfig config) : base(scheduler, config)
        {
            RegisterComponent(new LockstepStoreReplayLifecycleComponent(_clientComponent.Lockstep, _clientComponent.SceneController.CommandFactory, config.ReplayPath));

            RegisterClickValidation(config);
        }
    }
}