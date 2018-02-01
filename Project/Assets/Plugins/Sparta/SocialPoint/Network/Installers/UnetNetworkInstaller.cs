using System;
using SocialPoint.Dependency;
using SocialPoint.Network;

namespace SocialPoint.Network
{
    public class UnetNetworkInstaller : SubInstaller
    {
        [Serializable]
        public class UnetNetworkConfig
        {
            public string ServerAddress = UnetNetworkClient.DefaultServerAddr;
            public int ServerPort = UnetNetworkServer.DefaultPort;
        }

        [Serializable]
        public class SettingsData
        {
            public UnetNetworkConfig Config;
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            Container.Rebind<UnetNetworkServerFactory>().ToMethod<UnetNetworkServerFactory>(CreateUnetServerFactory);
            Container.Rebind<INetworkServerFactory>("internal").ToLookup<UnetNetworkServerFactory>();
            Container.Rebind<INetworkServerFactory>().ToLookup<UnetNetworkServerFactory>();

            Container.Rebind<UnetNetworkClientFactory>().ToMethod<UnetNetworkClientFactory>(CreateUnetClientFactory);
            Container.Rebind<INetworkClientFactory>("internal").ToLookup<UnetNetworkClientFactory>();
            Container.Rebind<INetworkClientFactory>().ToLookup<UnetNetworkClientFactory>();
        }

        UnetNetworkServerFactory CreateUnetServerFactory()
        {
            return new UnetNetworkServerFactory(Settings);
        }

        UnetNetworkClientFactory CreateUnetClientFactory()
        {
            return new UnetNetworkClientFactory(Settings);
        }
    }
}