using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class UnetNetworkServerFactory : INetworkServerFactory
    {
        readonly UnetNetworkInstaller.SettingsData _settings;
        readonly IUpdateScheduler _updateScheduler;
        readonly List<INetworkServerDelegate> _delegates;

        public UnetNetworkServerFactory(UnetNetworkInstaller.SettingsData settings, IUpdateScheduler updateScheduler, List<INetworkServerDelegate> delegates)
        {
            _settings = settings;
            _updateScheduler = updateScheduler;
            _delegates = delegates;
        }

        #region INetworkServerFactory implementation

        INetworkServer INetworkServerFactory.Create()
        {
            return Create();
        }

        #endregion

        public UnetNetworkServer Create()
        {
            var server = new UnetNetworkServer(_updateScheduler, _settings.Config.ServerPort);
            SetupServer(server);

            return server;
        }

        void SetupServer(INetworkServer server)
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                server.AddDelegate(_delegates[i]);
            }
        }
    }
}

