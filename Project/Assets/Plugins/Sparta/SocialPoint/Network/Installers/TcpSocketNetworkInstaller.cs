using System;
using SocialPoint.Dependency;
using SocialPoint.Network;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class TcpSocketNetworkInstaller : SubInstaller
    {
        [Serializable]
        public class TcpSocketNetworkConfig
        {
            public string ServerAddress = TcpSocketNetworkServer.DefaultAddress;
            public int ServerPort = TcpSocketNetworkServer.DefaultPort;
        }

        [Serializable]
        public class SettingsData
        {
            public TcpSocketNetworkConfig Config;
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            Container.Rebind<TcpSocketNetworkServerFactory>().ToMethod<TcpSocketNetworkServerFactory>(CreateTcpSocketNetworkServerFactory);
            Container.Rebind<INetworkServerFactory>("internal").ToLookup<TcpSocketNetworkServerFactory>();
            Container.Rebind<INetworkServerFactory>().ToLookup<TcpSocketNetworkServerFactory>();

            Container.Rebind<TcpSocketNetworkClientFactory>().ToMethod<TcpSocketNetworkClientFactory>(CreateTcpSocketNetworkClientFactory);
            Container.Rebind<INetworkClientFactory>("internal").ToLookup<TcpSocketNetworkClientFactory>();
            Container.Rebind<INetworkClientFactory>().ToLookup<TcpSocketNetworkClientFactory>();
        }

        TcpSocketNetworkServerFactory CreateTcpSocketNetworkServerFactory()
        {
            return new TcpSocketNetworkServerFactory(Settings,
                Container.Resolve<IUpdateScheduler>(),
                Container.ResolveList<INetworkServerDelegate>());
        }

        TcpSocketNetworkClientFactory CreateTcpSocketNetworkClientFactory()
        {
            return new TcpSocketNetworkClientFactory(Settings,
                Container.Resolve<IUpdateScheduler>(),
                Container.ResolveList<INetworkClientDelegate>());
        }
    }
}