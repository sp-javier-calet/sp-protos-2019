using SocialPoint.Dependency;

namespace SocialPoint.Network
{
    public class LocalBridgeNetworkInstaller : SubInstaller
    {
        public PhotonNetworkInstaller.SettingsData PhotonSettings = new PhotonNetworkInstaller.SettingsData();

        public override void InstallBindings()
        {
            // Server
            Container.RebindUnityComponent<PhotonNetworkServer>().WithSetup<PhotonNetworkServer>(SetupPhotonServer);
            Container.Rebind<LocalNetworkServer>().ToMethod<LocalNetworkServer>(CreateLocalServer);
            Container.Rebind<LocalBridgeNetworkServer>().ToMethod<LocalBridgeNetworkServer>(CreateLocalBridgeServer);
            Container.Rebind<INetworkServer>().ToLookup<LocalBridgeNetworkServer>();
            Container.Rebind<ILocalNetworkServer>().ToLookup<LocalBridgeNetworkServer>();

            // Client
            Container.Rebind<LocalNetworkClient>().ToMethod<LocalNetworkClient>(CreateLocalClient);
            Container.Rebind<INetworkClient>().ToLookup<LocalNetworkClient>();
        }

        void SetupPhotonServer(PhotonNetworkServer server)
        {
            server.Config = PhotonSettings.Config;
        }

        LocalNetworkServer CreateLocalServer()
        {
            return new LocalNetworkServer();
        }

        LocalBridgeNetworkServer CreateLocalBridgeServer()
        {
            var netServer = Container.Resolve<PhotonNetworkServer>();
            var localServer = Container.Resolve<LocalNetworkServer>();

            var server = new LocalBridgeNetworkServer(netServer, localServer);
            SetupServer(server);
            return server;
        }

        void SetupServer(INetworkServer server)
        {
            var dlgs = Container.ResolveList<INetworkServerDelegate>();
            for (var i = 0; i < dlgs.Count; i++)
            {
                server.AddDelegate(dlgs[i]);
            }
        }

        LocalNetworkClient CreateLocalClient()
        {
            return new LocalNetworkClient(Container.Resolve<ILocalNetworkServer>());
        }
    }
}
