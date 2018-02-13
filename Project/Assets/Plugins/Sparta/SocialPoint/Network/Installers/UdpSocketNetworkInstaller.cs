using System;
using SocialPoint.Utils;
using SocialPoint.Dependency;
using SocialPoint.Network;

namespace SocialPoint.Network
{
    public class UdpSocketNetworkInstaller : SubInstaller
    {
        [Serializable]
        public class UdpSocketNetworkConfig
        {
            public string ServerAddress = UdpSocketNetworkServer.DefaultAddress;
            public int ServerPort = UdpSocketNetworkServer.DefaultPort;
            public int PeerLimit = UdpSocketNetworkServer.DefaultPeerLimit;
            public string ConnectionKey = UdpSocketNetworkServer.DefaultConnectionKey;
            public int UpdateTime = UdpSocketNetworkServer.DefaultUpdateTime;
        }

        [Serializable]
        public class SettingsData
        {
            public UdpSocketNetworkConfig Config;
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            Container.Rebind<UdpSocketNetworkServerFactory>().ToMethod<UdpSocketNetworkServerFactory>(CreateUdpSocketNetworkServerFactory);
            Container.Rebind<INetworkServerFactory>("internal").ToLookup<UdpSocketNetworkServerFactory>();
            Container.Rebind<INetworkServerFactory>().ToLookup<UdpSocketNetworkServerFactory>();

            Container.Rebind<UdpSocketNetworkClientFactory>().ToMethod<UdpSocketNetworkClientFactory>(CreateUdpSocketNetworkClientFactory);
            Container.Rebind<INetworkClientFactory>("internal").ToLookup<UdpSocketNetworkClientFactory>();
            Container.Rebind<INetworkClientFactory>().ToLookup<UdpSocketNetworkClientFactory>();
        }

        UdpSocketNetworkServerFactory CreateUdpSocketNetworkServerFactory()
        {
            return new UdpSocketNetworkServerFactory(Settings,
                Container.Resolve<IUpdateScheduler>(),
                Container.ResolveList<INetworkServerDelegate>());
        }

        UdpSocketNetworkClientFactory CreateUdpSocketNetworkClientFactory()
        {
            return new UdpSocketNetworkClientFactory(Settings,
                Container.Resolve<IUpdateScheduler>(),
                Container.ResolveList<INetworkClientDelegate>());
        }

    }
}