using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class TcpSocketNetworkServerFactory : BaseNetworkServerFactory, INetworkServerFactory
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

        #region INetworkServerFactory implementation

        INetworkServer INetworkServerFactory.Create()
        {
            return Create();
        }

        #endregion

        public TcpSocketNetworkServer Create()
        {
            return Create<TcpSocketNetworkServer>(new TcpSocketNetworkServer(_updateScheduler,
                _settings.Config.ServerAddress, _settings.Config.ServerPort));
        }
    }
}

