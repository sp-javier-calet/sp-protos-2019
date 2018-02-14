using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class NetworkStatsClientFactory : BaseNetworkClientFactory<NetworkStatsClient>
    {
        readonly INetworkClientFactory _clientFactory;
        readonly NetworkStatsInstaller.SettingsData _settings;
        readonly IUpdateScheduler _updateScheduler;

        public NetworkStatsClientFactory(INetworkClientFactory clientFactory, NetworkStatsInstaller.SettingsData settings, IUpdateScheduler updateScheduler)
        {
            _clientFactory = clientFactory;
            _settings = settings;
            _updateScheduler = updateScheduler;
        }

        protected override NetworkStatsClient DoCreate()
        {
            var client = new NetworkStatsClient(_clientFactory.Create(), _updateScheduler);
            client.PingInterval = _settings.PingInterval;

            return client;
        }
    }
}