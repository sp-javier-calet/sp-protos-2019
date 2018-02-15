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
            Container.Rebind<UdpSocketNetworkServer>().ToMethod<UdpSocketNetworkServer>(CreateSocketServer, SetupServer);
            Container.Bind<IDisposable>().ToLookup<UdpSocketNetworkServer>();
            Container.Rebind<INetworkServer>("internal").ToLookup<UdpSocketNetworkServer>();
            Container.Rebind<INetworkServer>().ToLookup<UdpSocketNetworkServer>();

            Container.Rebind<UdpSocketNetworkClient>().ToMethod<UdpSocketNetworkClient>(CreateSocketClient, SetupClient);
            Container.Bind<IDisposable>().ToLookup<UdpSocketNetworkClient>();
            Container.Rebind<INetworkClient>("internal").ToLookup<UdpSocketNetworkClient>();
            Container.Rebind<INetworkClient>().ToLookup<UdpSocketNetworkClient>();
        }

        UdpSocketNetworkClient CreateSocketClient()
        {
            UdpSocketNetworkClient socketClient =  new UdpSocketNetworkClient(
                Container.Resolve<IUpdateScheduler>(),
                Settings.Config.ConnectionKey, 
                Settings.Config.UpdateTime);
            return socketClient; 
        }

        UdpSocketNetworkServer CreateSocketServer()
        {
            UdpSocketNetworkServer socketServer = new UdpSocketNetworkServer(
                Container.Resolve<IUpdateScheduler>(),
                Settings.Config.PeerLimit,
                Settings.Config.ConnectionKey,
                Settings.Config.UpdateTime);
            return socketServer; 
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