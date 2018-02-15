using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class UdpSocketNetworkClientFactory : BaseNetworkClientFactory<UdpSocketNetworkClient>
    {
        readonly UdpSocketNetworkInstaller.SettingsData _settings;
        readonly IUpdateScheduler _updateScheduler;

        public UdpSocketNetworkClientFactory(
            UdpSocketNetworkInstaller.SettingsData settings,
            IUpdateScheduler updateScheduler,
            List<INetworkClientDelegate> delegates) : base(delegates)
        {
            _settings = settings;
            _updateScheduler = updateScheduler;
        }

        protected override UdpSocketNetworkClient DoCreate()
        {
            return new UdpSocketNetworkClient(
                _updateScheduler,
                _settings.Config.ConnectionKey, 
                _settings.Config.UpdateTime
            );
        }
    }
}