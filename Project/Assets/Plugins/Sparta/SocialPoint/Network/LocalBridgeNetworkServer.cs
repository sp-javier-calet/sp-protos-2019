using SocialPoint.Base;
using SocialPoint.Dependency;

namespace SocialPoint.Network
{
    class LocalBridgeNetworkServer : ILocalNetworkServer
    {
        readonly INetworkServer _netServer;
        readonly ILocalNetworkServer _localServer;

        public bool Running
        {
            get
            {
                return _netServer.Running;
            }
        }

        public string Id
        {
            get
            {
                return _netServer.Id;
            }
        }

        public LocalBridgeNetworkServer(INetworkServer netServer, ILocalNetworkServer localServer)
        {
            _netServer = netServer;
            _localServer = localServer;
        }

        public void Start()
        {
            if(!_netServer.Running)
            {
                _netServer.Start();
            }

            if(!_localServer.Running)
            {
                _localServer.Start();
            }
        }

        public void Stop()
        {
            if(_netServer.Running)
            {
                _netServer.Stop();
            }

            if(_localServer.Running)
            {
                _localServer.Stop();
            }
        }

        public void Fail(Error err)
        {
            _netServer.Fail(err);
            _localServer.Fail(err);
        }

        public INetworkMessage CreateMessage(NetworkMessageData info)
        {
            return CreateLocalMessage(info);
        }

        public ILocalNetworkMessage CreateLocalMessage(NetworkMessageData info)
        {
            return new LocalBridgeNetworkMessage(info, _netServer, _localServer);
        }

        public void OnLocalMessageReceived(LocalNetworkClient origin, ILocalNetworkMessage msg)
        {
            _localServer.OnLocalMessageReceived(origin, msg);
        }

        public byte OnClientConnecting(LocalNetworkClient client)
        {
            return _localServer.OnClientConnecting(client);
        }

        public void OnClientConnected(LocalNetworkClient client)
        {
            _localServer.OnClientConnected(client);
        }

        public void OnClientDisconnected(LocalNetworkClient client)
        {
            _localServer.OnClientDisconnected(client);
        }

        public void AddDelegate(INetworkServerDelegate dlg)
        {
            _netServer.AddDelegate(dlg);
            _localServer.AddDelegate(dlg);
        }

        public void RemoveDelegate(INetworkServerDelegate dlg)
        {
            _netServer.RemoveDelegate(dlg);
            _localServer.RemoveDelegate(dlg);
        }

        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            _netServer.RegisterReceiver(receiver);
            _localServer.RegisterReceiver(receiver);
        }

        public int GetTimestamp()
        {
            return _netServer.GetTimestamp();
        }

        public bool LatencySupported
        {
            get
            {
                return _netServer.LatencySupported;
            }
        }

        void System.IDisposable.Dispose()
        {
            Stop();
            _netServer.Dispose();
            _localServer.Dispose();
        }
    }

    class LocalBridgeNetworkServerFactory : ILocalNetworkServerFactory
    {
        readonly PhotonNetworkInstaller.SettingsData _settings;
        readonly INetworkServerFactory _photonNetworkServerFactory;
        readonly INetworkServerFactory _localNetworkServerFactory;

        public LocalBridgeNetworkServerFactory(PhotonNetworkInstaller.SettingsData settings,
            INetworkServerFactory photonNetworkServerFactory,
            INetworkServerFactory localNetworkServerFactory)
        {
            _settings = settings;
            _photonNetworkServerFactory = photonNetworkServerFactory;
            _localNetworkServerFactory = localNetworkServerFactory;
        }

        #region INetworkServerFactory implementation

        INetworkServer INetworkServerFactory.Create()
        {
            var netServer = _photonNetworkServerFactory.Create();
            var localServer = _localNetworkServerFactory.Create();

            SetupPhotonServer((PhotonNetworkServer)netServer);

            var server = new LocalBridgeNetworkServer(netServer, (ILocalNetworkServer)localServer);
            SetupServer(server);

            return server;
        }

        #endregion

        void SetupPhotonServer(PhotonNetworkServer server)
        {
            server.Config = _settings.Config;
        }

        void SetupServer(INetworkServer server)
        {
            var dlgs = Services.Instance.ResolveList<INetworkServerDelegate>();
            for (var i = 0; i < dlgs.Count; i++)
            {
                server.AddDelegate(dlgs[i]);
            }
        }
    }
}
