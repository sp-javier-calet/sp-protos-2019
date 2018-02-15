using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class TcpSocketNetworkServerFactory : BaseNetworkServerFactory<TcpSocketNetworkServer>
    {
        readonly TcpSocketNetworkInstaller.SettingsData _settings;
        readonly IUpdateScheduler _updateScheduler;

        public TcpSocketNetworkServerFactory(
            TcpSocketNetworkInstaller.SettingsData settings,
            IUpdateScheduler updateScheduler,
            List<INetworkServerDelegate> delegates) : base(delegates)
        {
            _settings = settings;
            _updateScheduler = updateScheduler;
        }

        protected override TcpSocketNetworkServer DoCreate()
        {
            var server = new TcpSocketNetworkServer(_updateScheduler,
                _settings.Config.ServerAddress, _settings.Config.ServerPort);

            return server;
        }
    }
}