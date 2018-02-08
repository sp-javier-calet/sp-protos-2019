using System.Collections.Generic;

namespace SocialPoint.Network
{
    public class LocalBridgeNetworkServerFactory : ILocalNetworkServerFactory
    {
        readonly PhotonNetworkInstaller.SettingsData _settings;
        readonly INetworkServerFactory _photonNetworkServerFactory;
        readonly INetworkServerFactory _localNetworkServerFactory;
        readonly List<INetworkServerDelegate> _delegates;

        public ILocalNetworkServer Server { get; private set; }

        public LocalBridgeNetworkServerFactory(PhotonNetworkInstaller.SettingsData settings,
            INetworkServerFactory photonNetworkServerFactory,
            INetworkServerFactory localNetworkServerFactory,
            List<INetworkServerDelegate> delegates)
        {
            _settings = settings;
            _photonNetworkServerFactory = photonNetworkServerFactory;
            _localNetworkServerFactory = localNetworkServerFactory;
            _delegates = delegates;
        }

        #region INetworkServerFactory implementation

        INetworkServer INetworkServerFactory.Create()
        {
            var netServer = _photonNetworkServerFactory.Create();
            var localServer = _localNetworkServerFactory.Create();

            SetupPhotonServer((PhotonNetworkServer)netServer);

            Server = new LocalBridgeNetworkServer(netServer, (ILocalNetworkServer)localServer);
            SetupServer(Server);

            return Server;
        }

        #endregion

        void SetupPhotonServer(PhotonNetworkServer server)
        {
            server.Config = _settings.Config;
            server.Config.CreateRoom = true;
        }

        void SetupServer(INetworkServer server)
        {
            for (var i = 0; i < _delegates.Count; i++)
            {
                server.AddDelegate(_delegates[i]);
            }
        }
    }}

