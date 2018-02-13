using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class TcpSocketNetworkClientFactory : BaseNetworkClientFactory, INetworkClientFactory
    {
        readonly TcpSocketNetworkInstaller.SettingsData _settings;
        readonly IUpdateScheduler _updateScheduler;

        public TcpSocketNetworkClientFactory(
            TcpSocketNetworkInstaller.SettingsData settings,
            IUpdateScheduler updateScheduler,
            List<INetworkClientDelegate> delegates) : base(delegates)
        {
            _settings = settings;
            _updateScheduler = updateScheduler;
        }

        #region INetworkClientFactory implementation

        INetworkClient INetworkClientFactory.Create()
        {
            return Create();
        }

        #endregion

        public TcpSocketNetworkClient Create()
        {
            return Create<TcpSocketNetworkClient>(new TcpSocketNetworkClient(_updateScheduler,
                _settings.Config.ServerAddress, _settings.Config.ServerPort));
        }
    }
}