using System;
using SocialPoint.Dependency;
using SocialPoint.Network;

namespace SocialPoint.Network
{
    public class LocalNetworkInstaller : SubInstaller
    {
        [Serializable]
        public class DelaySettingsData
        {
            public float Average = 0.0f;
            public float Variance = 0.0f;
        }

        [Serializable]
        public class ClientSettingsData
        {
            public DelaySettingsData EmissionDelay;
            public DelaySettingsData ReceptionDelay;
        }


        [Serializable]
        public class ServerSettingsData
        {
            public DelaySettingsData EmissionDelay;
            public DelaySettingsData ReceptionDelay;
        }

        [Serializable]
        public class SettingsData
        {
            public ClientSettingsData Client;
            public ServerSettingsData Server;
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            Container.Rebind<LocalNetworkServerFactory>().ToMethod<LocalNetworkServerFactory>(CreateLocalServerFactory);
            Container.Rebind<SimulateNetworkServerFactory>().ToMethod<SimulateNetworkServerFactory>(CreateSimulateServerFactory);
            Container.Rebind<INetworkServerFactory>("internal").ToLookup<SimulateNetworkServerFactory>();
            Container.Rebind<INetworkServerFactory>().ToLookup<LocalNetworkServerFactory>();
            Container.Rebind<ILocalNetworkServerFactory>().ToLookup<LocalNetworkServerFactory>();

            Container.Rebind<LocalNetworkClientFactory>().ToMethod<LocalNetworkClientFactory>(CreateLocalClientFactory);
            Container.Rebind<SimulateNetworkClientFactory>().ToMethod<SimulateNetworkClientFactory>(CreateSimulateClientFactory);
            Container.Rebind<INetworkClientFactory>("internal").ToLookup<SimulateNetworkClientFactory>();
            Container.Rebind<INetworkClientFactory>().ToLookup<LocalNetworkClientFactory>();
        }

        LocalNetworkServerFactory CreateLocalServerFactory()
        {
            return new LocalNetworkServerFactory();
        }

        LocalNetworkClientFactory CreateLocalClientFactory()
        {
            return new LocalNetworkClientFactory(Container.Resolve<ILocalNetworkServerFactory>());
        }

        SimulateNetworkServerFactory CreateSimulateServerFactory()
        {
            return new SimulateNetworkServerFactory(Container.Resolve<LocalNetworkServerFactory>(), Settings.Server);
        }

        SimulateNetworkClientFactory CreateSimulateClientFactory()
        {
            return new SimulateNetworkClientFactory(Container.Resolve<LocalNetworkClientFactory>(), Settings.Client);
        }
    }
}
