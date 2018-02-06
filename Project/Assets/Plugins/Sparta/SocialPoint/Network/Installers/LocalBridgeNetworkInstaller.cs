using SocialPoint.Dependency;

namespace SocialPoint.Network
{
    public class LocalBridgeNetworkInstaller : SubInstaller
    {
        public PhotonNetworkInstaller.SettingsData Settings = new PhotonNetworkInstaller.SettingsData();

        public override void InstallBindings()
        {
            // Server
            Container.Rebind<PhotonNetworkServerFactory>().ToMethod<PhotonNetworkServerFactory>(CreatePhotonServerFactory);
            Container.Rebind<LocalNetworkServerFactory>().ToMethod<LocalNetworkServerFactory>(CreateLocalServerFactory);
            Container.Rebind<LocalBridgeNetworkServerFactory>().ToMethod<LocalBridgeNetworkServerFactory>(CreateLocalBridgeNetworkServerFactory);
            Container.Rebind<INetworkServerFactory>("internal").ToLookup<LocalBridgeNetworkServerFactory>();
            Container.Rebind<INetworkServerFactory>().ToLookup<LocalBridgeNetworkServerFactory>();
            Container.Rebind<ILocalNetworkServerFactory>().ToLookup<LocalBridgeNetworkServerFactory>();

            // Client
            Container.Rebind<LocalNetworkClientFactory>().ToMethod<LocalNetworkClientFactory>(CreateLocalClientFactory);
            Container.Rebind<INetworkClientFactory>("internal").ToLookup<LocalNetworkClientFactory>();
            Container.Rebind<INetworkClientFactory>().ToLookup<LocalNetworkClientFactory>();
        }

        PhotonNetworkServerFactory CreatePhotonServerFactory()
        {
            return new PhotonNetworkServerFactory(Settings, false);
        }

        LocalNetworkServerFactory CreateLocalServerFactory()
        {
            return new LocalNetworkServerFactory();
        }

        LocalBridgeNetworkServerFactory CreateLocalBridgeNetworkServerFactory()
        {
            return new LocalBridgeNetworkServerFactory(Settings,
                Container.Resolve<PhotonNetworkServerFactory>(),
                Container.Resolve<LocalNetworkServerFactory>());
        }

        LocalNetworkClientFactory CreateLocalClientFactory()
        {
            return new LocalNetworkClientFactory(Container.Resolve<LocalNetworkServerFactory>());
        }

    }
}
