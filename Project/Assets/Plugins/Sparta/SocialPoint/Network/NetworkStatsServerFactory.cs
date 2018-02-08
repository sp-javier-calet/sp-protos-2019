using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class NetworkStatsServerFactory : INetworkServerFactory
    {
        readonly INetworkServerFactory _serverFactory;
        readonly IUpdateScheduler _updateScheduler;

        public NetworkStatsServerFactory(INetworkServerFactory serverFactory, IUpdateScheduler updateScheduler)
        {
            _serverFactory = serverFactory;
            _updateScheduler = updateScheduler;
        }

        #region INetworkServerFactory implementation

        INetworkServer INetworkServerFactory.Create()
        {
            return new NetworkStatsServer(
                _serverFactory.Create(),
                _updateScheduler
            );
        }

        #endregion
    }
}