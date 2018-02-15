using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class SimulateNetworkClientFactory : BaseNetworkClientFactory<SimulateNetworkClient>
    {
        readonly INetworkClientFactory _clientFactory;
        readonly LocalNetworkInstaller.ClientSettingsData _settings;
        readonly IUpdateScheduler _updateScheduler;

        public SimulateNetworkClientFactory(
            INetworkClientFactory clientFactory,
            LocalNetworkInstaller.ClientSettingsData settings,
            IUpdateScheduler updateScheduler,
            List<INetworkClientDelegate> delegates) : base(delegates)
        {
            _clientFactory = clientFactory;
            _settings = settings;
            _updateScheduler = updateScheduler;
        }

        protected override SimulateNetworkClient DoCreate()
        {
            var client = new SimulateNetworkClient(_clientFactory.Create(), _updateScheduler);
            SetupClient(client);

            return client;
        }

        void SetupClient(SimulateNetworkClient client)
        {
            client.ReceptionDelay = _settings.ReceptionDelay.Average;
            client.ReceptionDelayVariance = _settings.ReceptionDelay.Variance;
            client.EmissionDelay = _settings.EmissionDelay.Average;
            client.EmissionDelayVariance = _settings.EmissionDelay.Variance;
        }
    }
}