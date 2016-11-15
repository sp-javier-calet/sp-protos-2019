using SocialPoint.Dependency;
using SocialPoint.Multiplayer;
using SocialPoint.Physics;
using SocialPoint.Network;

public class GameMultiplayerServerInstaller : Installer, IInitializable
{
    public override void InstallBindings()
    {
        Container.Bind<IInitializable>().ToInstance(this);

        #if UNITY_5
        Container.Rebind<UnityPhysicsDebugger>().ToMethod<UnityPhysicsDebugger>(CreateUnityPhysicsDebugger);
        Container.Rebind<IPhysicsDebugger>().ToLookup<UnityPhysicsDebugger>();
        #else
        Container.Rebind<EmptyPhysicsDebugger>().ToMethod<EmptyPhysicsDebugger>(CreateEmptyPhysicsDebugger);
        Container.Rebind<IPhysicsDebugger>().ToLookup<EmptyPhysicsDebugger>();
        #endif

        Container.Rebind<GameMultiplayerServerBehaviour>().ToMethod<GameMultiplayerServerBehaviour>(CreateServerBehaviour);
    }

    GameMultiplayerServerBehaviour CreateServerBehaviour()
    {
        return new GameMultiplayerServerBehaviour(
            Container.Resolve<INetworkServer>(),
            Container.Resolve<NetworkServerSceneController>(),
            Container.Resolve<IPhysicsDebugger>());
    }

    UnityPhysicsDebugger CreateUnityPhysicsDebugger()
    {
        return new UnityPhysicsDebugger();
    }

    EmptyPhysicsDebugger CreateEmptyPhysicsDebugger()
    {
        return new EmptyPhysicsDebugger();
    }

    public void Initialize()
    {
        Container.Resolve<GameMultiplayerServerBehaviour>();
    }
}