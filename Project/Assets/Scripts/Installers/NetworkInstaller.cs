using System;
using SocialPoint.Utils;
using SocialPoint.Dependency;
using SocialPoint.Network;
using SocialPoint.Multiplayer;
using SocialPoint.AdminPanel;

public class NetworkInstaller : Installer
{
    public enum NetworkTech
    {
        Local,
        Unet,
        Photon
    }

    [Serializable]
    public class UnetNetworkConfig
    {
        public string ServerAddress = UnetNetworkClient.DefaultServerAddr;
        public int ServerPort = UnetNetworkServer.DefaultPort;
    }

    [Serializable]
    public class SettingsData
    {
        public NetworkTech Tech = NetworkTech.Local;
        public UnetNetworkConfig UnetConfig;
        public PhotonNetworkConfig PhotonConfig;
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
        if (Settings.Tech == NetworkTech.Local)
        {
            Container.Rebind<LocalNetworkServer>().ToMethod<LocalNetworkServer>(CreateLocalServer, SetupServer);
            Container.Rebind<INetworkServer>().ToLookup<LocalNetworkServer>();
            Container.Rebind<LocalNetworkClient>().ToMethod<LocalNetworkClient>(CreateLocalClient, SetupClient);
            Container.Rebind<INetworkClient>().ToLookup<LocalNetworkClient>();
        }
        else if (Settings.Tech == NetworkTech.Unet)
        {
            Container.Rebind<UnetNetworkServer>().ToMethod<UnetNetworkServer>(CreateUnetServer, SetupServer);
            Container.Bind<IDisposable>().ToLookup<UnetNetworkServer>();
            Container.Rebind<INetworkServer>().ToLookup<UnetNetworkServer>();
            Container.Rebind<UnetNetworkClient>().ToMethod<UnetNetworkClient>(CreateUnetClient, SetupClient);
            Container.Bind<IDisposable>().ToLookup<UnetNetworkClient>();
            Container.Rebind<INetworkClient>().ToLookup<UnetNetworkClient>();
        }
        else if (Settings.Tech == NetworkTech.Photon)
        {
            Container.RebindUnityComponent<PhotonNetworkServer>().WithSetup<PhotonNetworkServer>(SetupPhotonServer);
            Container.Rebind<INetworkServer>().ToLookup<PhotonNetworkServer>();
            Container.RebindUnityComponent<PhotonNetworkClient>().WithSetup<PhotonNetworkServer>(SetupPhotonClient);
            Container.Rebind<INetworkClient>().ToLookup<PhotonNetworkClient>();
        }        
        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelNetwork>(CreateAdminPanel);
    }

    AdminPanelNetwork CreateAdminPanel()
    {
        return new AdminPanelNetwork(
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
            Settings.UnetConfig.ServerAddress, Settings.UnetConfig.ServerPort);
    }

    UnetNetworkServer CreateUnetServer()
    {
        return new UnetNetworkServer(
            Container.Resolve<IUpdateScheduler>(),
            Settings.UnetConfig.ServerPort);
    }

    void SetupServer(INetworkServer server)
    {
        var dlgs = Container.ResolveList<INetworkServerDelegate>();
        for(var i = 0; i < dlgs.Count; i++)
        {
            server.AddDelegate(dlgs[i]);
        }
    }

    void SetupClient(INetworkClient client)
    {
        var dlgs = Container.ResolveList<INetworkClientDelegate>();
        for(var i = 0; i < dlgs.Count; i++)
        {
            client.AddDelegate(dlgs[i]);
        }
    }

    void SetupPhotonServer(PhotonNetworkServer server)
    {
        server.Init(Settings.PhotonConfig);
        SetupServer(server);
    }

    void SetupPhotonClient(PhotonNetworkClient client)
    {
        client.Init(Settings.PhotonConfig);
        SetupClient(client);
    }
}
