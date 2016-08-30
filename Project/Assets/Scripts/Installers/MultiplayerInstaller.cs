using System;
using SocialPoint.Utils;
using SocialPoint.Dependency;
using SocialPoint.Network;
using SocialPoint.Multiplayer;
using SocialPoint.AdminPanel;

public class MultiplayerInstaller : Installer
{

    public enum MultiplayerTech
    {
        Local,
        Unet
    }

    [Serializable]
    public class SettingsData
    {
        public MultiplayerTech Tech = MultiplayerTech.Local;
        public string ServerAddress = UnetNetworkClient.DefaultServerAddr;
        public int ServerPort = UnetNetworkServer.DefaultPort;
        public string MultiplayerParentTag = "MultiplayerParent";
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
        if(Settings.Tech == MultiplayerTech.Local)
        {
            Container.Rebind<LocalNetworkServer>().ToMethod<LocalNetworkServer>(CreateLocalServer, SetupServer);
            Container.Rebind<INetworkServer>().ToLookup<LocalNetworkServer>();
            Container.Rebind<LocalNetworkClient>().ToMethod<LocalNetworkClient>(CreateLocalClient, SetupClient);
            Container.Rebind<INetworkClient>().ToLookup<LocalNetworkClient>();
        }
        else if(Settings.Tech == MultiplayerTech.Unet)
        {
            Container.Rebind<UnetNetworkServer>().ToMethod<UnetNetworkServer>(CreateUnetServer, SetupServer);
            Container.Bind<IDisposable>().ToLookup<UnetNetworkServer>();
            Container.Rebind<INetworkServer>().ToLookup<UnetNetworkServer>();
            Container.Rebind<UnetNetworkClient>().ToMethod<UnetNetworkClient>(CreateUnetClient, SetupClient);
            Container.Bind<IDisposable>().ToLookup<UnetNetworkClient>();
            Container.Rebind<INetworkClient>().ToLookup<UnetNetworkClient>();
        }

        Container.Rebind<NetworkServerSceneController>()
            .ToMethod<NetworkServerSceneController>(CreateServerSceneController, SetupServerSceneController);
        Container.Rebind<INetworkMessageReceiver>("server")
            .ToLookup<NetworkServerSceneController>();
        Container.Rebind<GameMultiplayerServerBehaviour>()
            .ToMethod<GameMultiplayerServerBehaviour>(CreateGameServer);
        Container.Rebind<INetworkServerSceneReceiver>()
            .ToLookup<GameMultiplayerServerBehaviour>();
        Container.Rebind<NetworkClientSceneController>()
            .ToMethod<NetworkClientSceneController>(CreateClientSceneController, SetupClientSceneController);
        Container.Rebind<INetworkMessageReceiver>("client")
            .ToLookup<NetworkClientSceneController>();
        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelMultiplayer>(CreateAdminPanel);
    }

    AdminPanelMultiplayer CreateAdminPanel()
    {
        return new AdminPanelMultiplayer(
            Container.Resolve<IUpdateScheduler>(), Container);
    }

    GameMultiplayerServerBehaviour CreateGameServer()
    {
        return new GameMultiplayerServerBehaviour(
            Container.Resolve<INetworkServer>(),
            Container.Resolve<NetworkServerSceneController>());
    }

    NetworkServerSceneController CreateServerSceneController()
    {
        return new UnityNetworkServerSceneController(
            Container.Resolve<INetworkServer>(),
            Container.Resolve<IUpdateScheduler>());
    }

    NetworkClientSceneController CreateClientSceneController()
    {
        return new UnityNetworkClientSceneController(
            Container.Resolve<INetworkClient>(),
            Settings.MultiplayerParentTag);
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

    void SetupServer(INetworkServer server)
    {
        var dlgs = Container.ResolveList<INetworkServerDelegate>();
        for(var i = 0; i < dlgs.Count; i++)
        {
            server.AddDelegate(dlgs[i]);
        }
        var receiver = Container.Resolve<INetworkMessageReceiver>("server");
        if(receiver != null)
        {
            server.RegisterReceiver(receiver);
        }
    }

    void SetupClient(INetworkClient client)
    {
        var dlgs = Container.ResolveList<INetworkClientDelegate>();
        for(var i = 0; i < dlgs.Count; i++)
        {
            client.AddDelegate(dlgs[i]);
        }
        var receiver = Container.Resolve<INetworkMessageReceiver>("client");
        if(receiver != null)
        {
            client.RegisterReceiver(receiver);
        }
    }

    void SetupServerSceneController(NetworkServerSceneController ctrl)
    {
        var behaviours = Container.ResolveList<INetworkServerSceneBehaviour>();
        for(var i = 0; i < behaviours.Count; i++)
        {
            ctrl.AddBehaviour(behaviours[i]);
        }
        var receiver = Container.Resolve<INetworkServerSceneReceiver>();
        if(receiver != null)
        {
            ctrl.RegisterReceiver(receiver);
        }
    }

    void SetupClientSceneController(NetworkClientSceneController ctrl)
    {
        var behaviours = Container.ResolveList<INetworkClientSceneBehaviour>();
        for(var i = 0; i < behaviours.Count; i++)
        {
            ctrl.AddBehaviour(behaviours[i]);
        }
        var receiver = Container.Resolve<INetworkClientSceneReceiver>();
        if(receiver != null)
        {
            ctrl.RegisterReceiver(receiver);
        }
    }
}
