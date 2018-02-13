using System.Collections.Generic;

namespace SocialPoint.Network
{
    public class SimulateNetworkServerFactory : BaseNetworkServerFactory, INetworkServerFactory
    {
        readonly INetworkServerFactory _serverFactory;
        readonly LocalNetworkInstaller.ServerSettingsData _settings;

        public SimulateNetworkServerFactory(
            INetworkServerFactory serverFactory,
            LocalNetworkInstaller.ServerSettingsData settings,
            List<INetworkServerDelegate> delegates) : base(delegates)
        {
            _serverFactory = serverFactory;
            _settings = settings;
        }

        #region INetworkServerFactory implementation

        INetworkServer INetworkServerFactory.Create()
        {
            return Create();
        }

        #endregion

        public SimulateNetworkServer Create()
        {
            var server = Create<SimulateNetworkServer>(new SimulateNetworkServer(_serverFactory.Create()));

            SetupServer(server);

            return server;
        }

        void SetupServer(SimulateNetworkServer server)
        {
            server.ReceptionDelay = _settings.ReceptionDelay.Average;
            server.ReceptionDelayVariance = _settings.ReceptionDelay.Variance;
            server.EmissionDelay = _settings.EmissionDelay.Average;
            server.EmissionDelayVariance = _settings.EmissionDelay.Variance;
        }
    }}