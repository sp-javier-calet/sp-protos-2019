using SocialPoint.Dependency;
using SocialPoint.Multiplayer;
using SocialPoint.Network;

public class GameMultiplayerServerInstaller : Installer, IInitializable
{
    public override void InstallBindings()
    {
        Container.Bind<IInitializable>().ToInstance(this);
        Container.Rebind<GameMultiplayerServerBehaviour>().ToMethod<GameMultiplayerServerBehaviour>(CreateServerBehaviour);
    }

    GameMultiplayerServerBehaviour CreateServerBehaviour()
    {
        return new GameMultiplayerServerBehaviour(
            Container.Resolve<INetworkServer>(),
            Container.Resolve<NetworkServerSceneController>());
    }

    public void Initialize()
    {
        Container.Resolve<GameMultiplayerServerBehaviour>();
    }
}
