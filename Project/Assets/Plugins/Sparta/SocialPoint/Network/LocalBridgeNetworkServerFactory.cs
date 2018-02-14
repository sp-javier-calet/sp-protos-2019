using System.Collections.Generic;

namespace SocialPoint.Network
{
    public class LocalBridgeNetworkServerFactory : BaseNetworkServerFactory<LocalBridgeNetworkServer>
    {
        readonly PhotonNetworkInstaller.SettingsData _settings;
        readonly INetworkServerFactory _photonNetworkServerFactory;
        readonly INetworkServerFactory _localNetworkServerFactory;

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

        protected override LocalBridgeNetworkServer DoCreate()
        {
            var netServer = _photonNetworkServerFactory.Create();
            var localServer = _localNetworkServerFactory.Create();

            SetupPhotonServer((PhotonNetworkServer)netServer);

            return new LocalBridgeNetworkServer(netServer, (ILocalNetworkServer)localServer);
        }

        void SetupPhotonServer(PhotonNetworkServer server)
        {
            server.Config = _settings.Config;
            server.Config.CreateRoom = true;
        }
    }}

