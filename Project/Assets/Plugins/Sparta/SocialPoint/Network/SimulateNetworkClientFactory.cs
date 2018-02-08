using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class SimulateNetworkClientFactory : INetworkClientFactory
    {
        readonly INetworkClientFactory _clientFactory;
        readonly LocalNetworkInstaller.ClientSettingsData _settings;
        readonly IUpdateScheduler _updateScheduler;
        readonly List<INetworkClientDelegate> _delegates;

        public SimulateNetworkClientFactory(INetworkClientFactory clientFactory, LocalNetworkInstaller.ClientSettingsData settings, IUpdateScheduler updateScheduler, List<INetworkClientDelegate> delegates)
        {
            _clientFactory = clientFactory;
            _settings = settings;
            _updateScheduler = updateScheduler;
            _delegates = delegates;
        }

        #region INetworkClientFactory implementation

        INetworkClient INetworkClientFactory.Create()
        {
            var client = new SimulateNetworkClient(_clientFactory.Create(), _updateScheduler);
            SetupClient(client);

            return client;
        }

        #endregion

        void SetupClient(SimulateNetworkClient client)
        {
            client.ReceptionDelay = _settings.ReceptionDelay.Average;
            client.ReceptionDelayVariance = _settings.ReceptionDelay.Variance;
            client.EmissionDelay = _settings.EmissionDelay.Average;
            client.EmissionDelayVariance = _settings.EmissionDelay.Variance;

            for(var i = 0; i < _delegates.Count; i++)
            {
                client.AddDelegate(_delegates[i]);
            }
        }
    }
}