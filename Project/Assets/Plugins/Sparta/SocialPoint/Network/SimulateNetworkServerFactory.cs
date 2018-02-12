using System.Collections.Generic;

namespace SocialPoint.Network
{
    public class SimulateNetworkServerFactory : INetworkServerFactory
    {
        readonly INetworkServerFactory _serverFactory;
        readonly LocalNetworkInstaller.ServerSettingsData _settings;
        readonly List<INetworkServerDelegate> _delegates;

        public SimulateNetworkServerFactory(INetworkServerFactory serverFactory, LocalNetworkInstaller.ServerSettingsData settings, List<INetworkServerDelegate> delegates)
        {
            _serverFactory = serverFactory;
            _settings = settings;
            _delegates = delegates;
        }

        #region INetworkServerFactory implementation

        INetworkServer INetworkServerFactory.Create()
        {
            return Create();
        }

        #endregion

        public SimulateNetworkServer Create()
        {
            var server = new SimulateNetworkServer(_serverFactory.Create());
            SetupServer(server);

            return server;
        }

        void SetupServer(SimulateNetworkServer server)
        {
            server.ReceptionDelay = _settings.ReceptionDelay.Average;
            server.ReceptionDelayVariance = _settings.ReceptionDelay.Variance;
            server.EmissionDelay = _settings.EmissionDelay.Average;
            server.EmissionDelayVariance = _settings.EmissionDelay.Variance;

            for(var i = 0; i < _delegates.Count; i++)
            {
                server.AddDelegate(_delegates[i]);
            }
        }
    }}