//-----------------------------------------------------------------------
// ReplayGameModeController.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using SocialPoint.Utils;

namespace Examples.Multiplayer.Lockstep
{
    public class ReplayGameModeController : BaseClientGameModeController
    {
        public ReplayGameModeController(IUpdateScheduler scheduler, ClientConfig config) : base(scheduler, config)
        {
            RegisterComponent(new LockstepPlayReplayLifecycleComponent(_clientComponent.Lockstep, _clientComponent.SceneController.CommandFactory, config.ReplayPath));
        }
    }
}