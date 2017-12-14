using System;
using SocialPoint.Utils;
using SocialPoint.Dependency;
using SocialPoint.Network;

namespace SocialPoint.Network
{
    public enum Protocol
    {
        TCP,
        UDP,
        ReliableUDP
    }

    public class SocketNetworkInstaller : SubInstaller
    {
        [Serializable]
        public class SocketNetworkConfig
        {
            public Protocol Protocol = Protocol.TCP;
            public string ServerAddress = SocketNetworkClient.DefaultServerAddr;
            public int ServerPort = SocketNetworkServer.DefaultPort;
        }

        [Serializable]
        public class SettingsData
        {
            public SocketNetworkConfig Config;
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            Container.Rebind<SocketNetworkServer>().ToMethod<SocketNetworkServer>(CreateSocketServer, SetupServer);
            Container.Bind<IDisposable>().ToLookup<SocketNetworkServer>();
            Container.Rebind<INetworkServer>("internal").ToLookup<SocketNetworkServer>();
            Container.Rebind<INetworkServer>().ToLookup<SocketNetworkServer>();

            Container.Rebind<SocketNetworkClient>().ToMethod<SocketNetworkClient>(CreateSocketClient, SetupClient);
            Container.Bind<IDisposable>().ToLookup<SocketNetworkClient>();
            Container.Rebind<INetworkClient>("internal").ToLookup<SocketNetworkClient>();
            Container.Rebind<INetworkClient>().ToLookup<SocketNetworkClient>();
        }

        SocketNetworkClient CreateSocketClient()
        {
            SocketNetworkClient socketClient = null;
            switch(Settings.Config.Protocol)
            {
            case Protocol.TCP:
                socketClient = new TCPSocketNetworkClient(
                    Settings.Config.ServerAddress, 
                    Settings.Config.ServerPort);
                break;
            case Protocol.UDP:
                break;
            case Protocol.ReliableUDP:
                socketClient = new UDPReliableSocketNetworkClient(
                    Settings.Config.ServerAddress, 
                    Settings.Config.ServerPort);
                break;
            }
            return socketClient; 
        }

        SocketNetworkServer CreateSocketServer()
        {
            SocketNetworkServer socketServer = null;
            switch(Settings.Config.Protocol)
            {
            case Protocol.TCP:
                socketServer = new TCPSocketNetworkServer(
                    Container.Resolve<IUpdateScheduler>(),
                    Settings.Config.ServerAddress, 
                    Settings.Config.ServerPort);
                break;
            case Protocol.UDP:
                break;
            case Protocol.ReliableUDP:
                socketServer = new UDPReliableSocketNetworkServer(
                    Container.Resolve<IUpdateScheduler>(),
                    Settings.Config.ServerAddress, 
                    Settings.Config.ServerPort);
                break;
            }
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