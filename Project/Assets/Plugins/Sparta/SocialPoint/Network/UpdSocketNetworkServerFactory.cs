using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class UdpSocketNetworkServerFactory : BaseNetworkServerFactory<UdpSocketNetworkServer>
    {
        readonly UdpSocketNetworkInstaller.SettingsData _settings;
        readonly IUpdateScheduler _updateScheduler;

        public UdpSocketNetworkServerFactory(
            UdpSocketNetworkInstaller.SettingsData settings,
            IUpdateScheduler updateScheduler,
            List<INetworkServerDelegate> delegates) : base(delegates)
        {
            _settings = settings;
            _updateScheduler = updateScheduler;
        }

        protected override UdpSocketNetworkServer DoCreate()
        {
            return new UdpSocketNetworkServer(
                _updateScheduler,
                _settings.Config.PeerLimit,
                _settings.Config.ConnectionKey,
                _settings.Config.UpdateTime
            );
        }
    }
}