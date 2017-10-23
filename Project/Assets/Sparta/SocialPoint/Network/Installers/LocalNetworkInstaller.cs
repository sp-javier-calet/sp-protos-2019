using System;
using SocialPoint.Dependency;
using SocialPoint.Network;
using SocialPoint.Utils;

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
            Container.Rebind<LocalNetworkServer>().ToMethod<LocalNetworkServer>(CreateLocalServer);
            Container.Rebind<SimulateNetworkServer>().ToMethod<SimulateNetworkServer>(CreateServer, SetupServer);
            Container.Bind<IDeltaUpdateable>().ToLookup<SimulateNetworkServer>();
            Container.Rebind<INetworkServer>("internal").ToLookup<SimulateNetworkServer>();
            Container.Rebind<INetworkServer>().ToLookup<LocalNetworkServer>();

            Container.Rebind<LocalNetworkClient>().ToMethod<LocalNetworkClient>(CreateLocalClient);
            Container.Rebind<SimulateNetworkClient>().ToMethod<SimulateNetworkClient>(CreateClient, SetupClient);
            Container.Bind<IDeltaUpdateable>().ToLookup<SimulateNetworkClient>();
            Container.Rebind<INetworkClient>("internal").ToLookup<SimulateNetworkClient>();
            Container.Rebind<INetworkClient>().ToLookup<LocalNetworkClient>();
        }

        LocalNetworkClient CreateLocalClient()
        {
            return new LocalNetworkClient(
                Container.Resolve<LocalNetworkServer>());
        }

        SimulateNetworkClient CreateClient()
        {
            var client = new SimulateNetworkClient(
                Container.Resolve<LocalNetworkClient>());
            client.ReceptionDelay = Settings.Client.ReceptionDelay.Average;
            client.ReceptionDelayVariance = Settings.Client.ReceptionDelay.Variance;
            client.EmissionDelay = Settings.Client.EmissionDelay.Average;
            client.EmissionDelayVariance = Settings.Client.EmissionDelay.Variance;
            return client;
        }

        LocalNetworkServer CreateLocalServer()
        {
            return new LocalNetworkServer();
        }

        SimulateNetworkServer CreateServer()
        {
            var server = new SimulateNetworkServer(
                Container.Resolve<LocalNetworkServer>());
            server.ReceptionDelay = Settings.Server.ReceptionDelay.Average;
            server.ReceptionDelayVariance = Settings.Server.ReceptionDelay.Variance;
            server.EmissionDelay = Settings.Server.EmissionDelay.Average;
            server.EmissionDelayVariance = Settings.Server.EmissionDelay.Variance;
            return server;
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
    }
}
