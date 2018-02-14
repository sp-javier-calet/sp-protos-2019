using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class UnetNetworkServerFactory : BaseNetworkServerFactory<UnetNetworkServer>
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

        protected override UnetNetworkServer DoCreate()
        {
            return new UnetNetworkServer(_updateScheduler, _settings.Config.ServerPort);;
        }
    }
}

