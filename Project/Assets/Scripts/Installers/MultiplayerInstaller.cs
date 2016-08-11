using System;
using SocialPoint.Utils;
using SocialPoint.Dependency;
using SocialPoint.Multiplayer;
using SocialPoint.AdminPanel;

public class MultiplayerInstaller : Installer
{
    public enum MultiplayerMode
    {
        Local,
        Client,
        Server
    }

    [Serializable]
    public class SettingsData
    {
        public MultiplayerMode Mode = MultiplayerMode.Local;
        public string ServerAddress = UnetNetworkClient.DefaultServerAddr;
        public int ServerPort = UnetNetworkServer.DefaultPort;
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
        if(Settings.Mode == MultiplayerMode.Local)
        {
            Container.Rebind<LocalNetworkServer>().ToMethod<LocalNetworkServer>(CreateLocalServer);
            Container.Rebind<INetworkServer>().ToLookup<LocalNetworkServer>();
            Container.Rebind<LocalNetworkClient>().ToMethod<LocalNetworkClient>(CreateLocalClient);
            Container.Rebind<INetworkClient>().ToLookup<LocalNetworkClient>();
            Container.Rebind<NetworkServerSceneController>().ToMethod<NetworkServerSceneController>(CreateServerSceneController);
        }
        else if(Settings.Mode == MultiplayerMode.Server)
        {
            Container.Rebind<UnetNetworkServer>().ToMethod<UnetNetworkServer>(CreateUnetServer);
            Container.Bind<IDisposable>().ToLookup<UnetNetworkServer>();
            Container.Rebind<INetworkServer>().ToLookup<UnetNetworkServer>();
        }
        else if(Settings.Mode == MultiplayerMode.Client)
        {
            Container.Rebind<UnetNetworkClient>().ToMethod<UnetNetworkClient>(CreateUnetClient);
            Container.Bind<IDisposable>().ToLookup<UnetNetworkClient>();
            Container.Rebind<INetworkClient>().ToLookup<UnetNetworkClient>();
        }

        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelMultiplayer>(CreateAdminPanel);
    }

    NetworkServerSceneController CreateServerSceneController()
    {
        return new NetworkServerSceneController(
            Container.Resolve<INetworkServer>());
    }

    AdminPanelMultiplayer CreateAdminPanel()
    {
        return new AdminPanelMultiplayer(
            Container.Resolve<IUpdateScheduler>(), Container);
    }

    LocalNetworkClient CreateLocalClient()
    {
        return new LocalNetworkClient(
            Container.Resolve<LocalNetworkServer>());
    }

    LocalNetworkServer CreateLocalServer()
    {
        return new LocalNetworkServer();
    }

    UnetNetworkClient CreateUnetClient()
    {
        return new UnetNetworkClient(
            Settings.ServerAddress, Settings.ServerPort);
    }

    UnetNetworkServer CreateUnetServer()
    {
        return new UnetNetworkServer(
            Container.Resolve<IUpdateScheduler>(),
            Settings.ServerPort);
    }
}
