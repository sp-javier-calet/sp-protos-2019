using System;
using SocialPoint.Utils;
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
            Container.Rebind<UnetNetworkServer>().ToMethod<UnetNetworkServer>(CreateUnetServer, SetupServer);
            Container.Bind<IDisposable>().ToLookup<UnetNetworkServer>();
            Container.Rebind<INetworkServer>("internal").ToLookup<UnetNetworkServer>();
            Container.Rebind<INetworkServer>().ToLookup<UnetNetworkServer>();

            Container.Rebind<UnetNetworkClient>().ToMethod<UnetNetworkClient>(CreateUnetClient, SetupClient);
            Container.Bind<IDisposable>().ToLookup<UnetNetworkClient>();
            Container.Rebind<INetworkClient>("internal").ToLookup<UnetNetworkClient>();
            Container.Rebind<INetworkClient>().ToLookup<UnetNetworkClient>();
        }

        UnetNetworkClient CreateUnetClient()
        {
            return new UnetNetworkClient(
                Settings.Config.ServerAddress, Settings.Config.ServerPort);
        }

        UnetNetworkServer CreateUnetServer()
        {
            return new UnetNetworkServer(
                Container.Resolve<IUpdateScheduler>(),
                Settings.Config.ServerPort);
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