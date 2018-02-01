using System;
using SocialPoint.Dependency;

namespace SocialPoint.Network
{
    public class NetworkStatsInstaller : ServiceInstaller
    {
        [Serializable]
        public class SettingsData
        {
            public bool Rebind = true;
            public int PingInterval = NetworkStatsServer.DefaultSendStatusMessageInterval;
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            if(Settings.Rebind)
            {
                Container.Bind<INetworkServerFactory>().ToMethod<NetworkStatsServerFactory>(CreateNetworkStatsServerFactory);
                Container.Bind<INetworkClientFactory>().ToMethod<NetworkStatsClientFactory>(CreateNetworkStatsClientFactory);
            }
            else
            {
                Container.Bind<INetworkServerFactory>().ToLookup<NetworkStatsServerFactory>("internal");
                Container.Bind<INetworkClientFactory>().ToLookup<NetworkStatsClientFactory>("internal");
            }
        }

        NetworkStatsServerFactory CreateNetworkStatsServerFactory()
        {
            return new NetworkStatsServerFactory(Container.Resolve<INetworkServerFactory>("internal"));
        }

        NetworkStatsClientFactory CreateNetworkStatsClientFactory()
        {
            return new NetworkStatsClientFactory(Container.Resolve<INetworkClientFactory>("internal"), Settings);
        }
    }
}

