using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class TcpSocketNetworkClientFactory : BaseNetworkClientFactory<TcpSocketNetworkClient>
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

        protected override TcpSocketNetworkClient DoCreate()
        {
            return new TcpSocketNetworkClient(_updateScheduler,
                _settings.Config.ServerAddress, _settings.Config.ServerPort);
        }
    }
}