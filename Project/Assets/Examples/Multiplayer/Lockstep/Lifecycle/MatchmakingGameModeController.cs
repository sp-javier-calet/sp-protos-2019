using SocialPoint.Dependency;
using SocialPoint.Matchmaking;
using SocialPoint.Network;
using SocialPoint.Utils;

namespace Examples.Multiplayer.Lockstep
{
    public class MatchmakingGameModeController : BaseClientGameModeController
    {
        public MatchmakingGameModeController(IUpdateScheduler scheduler, ClientConfig config, IMatchmakingServer matchmakingServer = null) : base(scheduler, config)
        {
            var clientFactory = Services.Instance.Resolve<INetworkClientFactory>();
            var netClient = clientFactory.Create();

            var matchmakingComp = new MatchmakingClientLifecycleComponent(new EmptyMatchmakingClient());
            RegisterComponent(matchmakingComp);

            var netClientComponent = new NetworkClientLifecycleComponent(netClient);
            RegisterComponent(netClientComponent);

            RegisterComponent(new LockstepNetworkClientLifecycleComponent(_clientComponent.SceneController, netClientComponent.NetworkClient));

            RegisterComponent(new LockstepStoreReplayLifecycleComponent(_clientComponent.Lockstep, _clientComponent.SceneController.CommandFactory, config.ReplayPath));

            RegisterClickValidation(config);
        }
    }
}