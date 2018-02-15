using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class NetworkStatsServerFactory : BaseNetworkServerFactory<NetworkStatsServer>
    {
        readonly INetworkServerFactory _serverFactory;
        readonly IUpdateScheduler _updateScheduler;

        public NetworkStatsServerFactory(INetworkServerFactory serverFactory, IUpdateScheduler updateScheduler)
        {
            _serverFactory = serverFactory;
            _updateScheduler = updateScheduler;
        }

        protected override NetworkStatsServer DoCreate()
        {
            return new NetworkStatsServer(_serverFactory.Create(), _updateScheduler);
        }
    }
}