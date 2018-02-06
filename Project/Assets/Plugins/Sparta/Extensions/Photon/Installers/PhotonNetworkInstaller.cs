using System;
using SocialPoint.Dependency;

namespace SocialPoint.Network
{
    public class PhotonNetworkInstaller : SubInstaller
    {
        [Serializable]
        public class SettingsData
        {
            public PhotonNetworkConfig Config;
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            Container.Rebind<PhotonNetworkServerFactory>().ToMethod<PhotonNetworkServerFactory>(CreatePhotonServerFactory);
            Container.Rebind<INetworkServerFactory>("internal").ToLookup<PhotonNetworkServerFactory>();
            Container.Rebind<INetworkServerFactory>().ToLookup<PhotonNetworkServerFactory>();

            Container.Rebind<PhotonNetworkClientFactory>().ToMethod<PhotonNetworkClientFactory>(CreatePhotonClientFactory);
            Container.Rebind<INetworkClientFactory>("internal").ToLookup<PhotonNetworkClientFactory>();
            Container.Rebind<INetworkClientFactory>().ToLookup<PhotonNetworkClientFactory>();
        }

        PhotonNetworkServerFactory CreatePhotonServerFactory()
        {
            return new PhotonNetworkServerFactory(Settings);
        }

        PhotonNetworkClientFactory CreatePhotonClientFactory()
        {
            return new PhotonNetworkClientFactory(Settings);
        }
    }
}
