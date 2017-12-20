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
            public string ServerAddress = TcpSocketNetworkServer.DefaultAddress;
            public int ServerPort = TcpSocketNetworkServer.DefaultPort;
        }

        [Serializable]
        public class SettingsData
        {
            public SocketNetworkConfig Config;
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            Container.Rebind<TcpSocketNetworkServer>().ToMethod<TcpSocketNetworkServer>(CreateSocketServer, SetupServer);
            Container.Bind<IDisposable>().ToLookup<TcpSocketNetworkServer>();
            Container.Rebind<INetworkServer>("internal").ToLookup<TcpSocketNetworkServer>();
            Container.Rebind<INetworkServer>().ToLookup<TcpSocketNetworkServer>();

            Container.Rebind<TcpSocketNetworkClient>().ToMethod<TcpSocketNetworkClient>(CreateSocketClient, SetupClient);
            Container.Bind<IDisposable>().ToLookup<TcpSocketNetworkClient>();
            Container.Rebind<INetworkClient>("internal").ToLookup<TcpSocketNetworkClient>();
            Container.Rebind<INetworkClient>().ToLookup<TcpSocketNetworkClient>();
        }

        TcpSocketNetworkClient CreateSocketClient()
        {
            TcpSocketNetworkClient socketClient = null;
            switch(Settings.Config.Protocol)
            {
            case Protocol.TCP:
                socketClient = new TcpSocketNetworkClient(
                    Container.Resolve<IUpdateScheduler>(),
                    Settings.Config.ServerAddress, 
                    Settings.Config.ServerPort);
                break;
            case Protocol.UDP:
                break;
            case Protocol.ReliableUDP:
                break;
            }
            return socketClient; 
        }

        TcpSocketNetworkServer CreateSocketServer()
        {
            TcpSocketNetworkServer socketServer = null;
            switch(Settings.Config.Protocol)
            {
            case Protocol.TCP:
                socketServer = new TcpSocketNetworkServer(
                    Container.Resolve<IUpdateScheduler>(),
                    Settings.Config.ServerAddress, 
                    Settings.Config.ServerPort);
                break;
            case Protocol.UDP:
                break;
            case Protocol.ReliableUDP:
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