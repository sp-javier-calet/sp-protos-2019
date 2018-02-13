using System.Collections.Generic;

namespace SocialPoint.Network
{
    public class LocalBridgeNetworkServerFactory : BaseNetworkServerFactory, ILocalNetworkServerFactory
    {
        readonly PhotonNetworkInstaller.SettingsData _settings;
        readonly INetworkServerFactory _photonNetworkServerFactory;
        readonly INetworkServerFactory _localNetworkServerFactory;

        public ILocalNetworkServer Server
        {
            get
            {
                return (ILocalNetworkServer)_server;
            }
        }

        public LocalBridgeNetworkServerFactory(
            PhotonNetworkInstaller.SettingsData settings,
            INetworkServerFactory photonNetworkServerFactory,
            INetworkServerFactory localNetworkServerFactory,
            List<INetworkServerDelegate> delegates = null) : base(delegates)
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

            return Create<LocalBridgeNetworkServer>(new LocalBridgeNetworkServer(netServer, (ILocalNetworkServer)localServer));
        }

        #endregion

        void SetupPhotonServer(PhotonNetworkServer server)
        {
            server.Config = _settings.Config;
            server.Config.CreateRoom = true;
        }
    }}

