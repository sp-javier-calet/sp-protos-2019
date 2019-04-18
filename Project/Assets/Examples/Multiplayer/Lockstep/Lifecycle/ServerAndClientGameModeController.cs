//-----------------------------------------------------------------------
// ServerAndClientGameModeController.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using SocialPoint.Dependency;
using SocialPoint.Lifecycle;
using SocialPoint.Network;
using SocialPoint.Utils;

namespace Examples.Multiplayer.Lockstep
{
    public class ServerAndClientGameModeController : HostGameModeController
    {
        public ServerAndClientGameModeController(IUpdateScheduler scheduler, ClientConfig config) : base(scheduler, config)
        {
        }

        protected override void CreateComponents(IUpdateScheduler scheduler, ClientConfig config)
        {
            CreateServerComponents(scheduler, config);
            
            _clientComponent = new LockstepClientLifecycleComponent(scheduler, config);
            RegisterComponent(_clientComponent);
            
            RegisterComponent(new LockstepStoreReplayLifecycleComponent(_clientComponent.Lockstep, _clientComponent.SceneController.CommandFactory, config.ReplayPath));

            var clientFactory = Services.Instance.Resolve<INetworkClientFactory>();
            var netClient = clientFactory.Create();

            var netClientComponent = new NetworkClientLifecycleComponent(netClient);
            RegisterComponent(netClientComponent);

            RegisterComponent(new LockstepNetworkClientLifecycleComponent(_clientComponent.SceneController, netClientComponent.NetworkClient));

            RegisterClickValidation(config);
        }
    }
}