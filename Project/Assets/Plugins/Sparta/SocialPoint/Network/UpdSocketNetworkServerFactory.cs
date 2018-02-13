using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class UdpSocketNetworkServerFactory : BaseNetworkServerFactory, INetworkServerFactory
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

        #region INetworkServerFactory implementation

        INetworkServer INetworkServerFactory.Create()
        {
            return Create();
        }

        #endregion

        public UdpSocketNetworkServer Create()
        {
            return Create<UdpSocketNetworkServer>(
                new UdpSocketNetworkServer(
                    _updateScheduler,
                    _settings.Config.PeerLimit,
                    _settings.Config.ConnectionKey,
                    _settings.Config.UpdateTime
                )
            );
        }
    }
}

