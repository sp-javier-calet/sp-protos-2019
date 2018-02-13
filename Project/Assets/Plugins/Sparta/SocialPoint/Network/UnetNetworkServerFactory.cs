using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class UnetNetworkServerFactory : BaseNetworkServerFactory, INetworkServerFactory
    {
        readonly UnetNetworkInstaller.SettingsData _settings;
        readonly IUpdateScheduler _updateScheduler;

        public UnetNetworkServerFactory(
            UnetNetworkInstaller.SettingsData settings,
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

        public UnetNetworkServer Create()
        {
            return Create<UnetNetworkServer>(new UnetNetworkServer(_updateScheduler, _settings.Config.ServerPort));
        }
    }
}

