using SocialPoint.Lifecycle;

namespace Examples.Multiplayer.Lockstep
{
    public interface IGameModeController : ILifecycleController
    {
        GameNetworkSceneController SceneController { get; }
        IEventProcessor Events { get; }
    }
}