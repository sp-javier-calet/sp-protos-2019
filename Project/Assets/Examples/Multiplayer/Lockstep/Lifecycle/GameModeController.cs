//-----------------------------------------------------------------------
// GameModeController.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using SocialPoint.Lifecycle;

namespace Examples.Multiplayer.Lockstep
{
    public interface IGameModeController : ILifecycleController
    {
        GameNetworkSceneController SceneController { get; }
        IEventProcessor Events { get; }
    }
}