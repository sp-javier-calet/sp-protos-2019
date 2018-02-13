using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class UdpSocketNetworkClientFactory : BaseNetworkClientFactory, INetworkClientFactory
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

        #region INetworkClientFactory implementation

        INetworkClient INetworkClientFactory.Create()
        {
            return Create();
        }

        #endregion

        public UdpSocketNetworkClient Create()
        {
            return Create<UdpSocketNetworkClient>(
                new UdpSocketNetworkClient(
                    _updateScheduler,
                    _settings.Config.ConnectionKey, 
                    _settings.Config.UpdateTime
                )
            );
        }
    }
}