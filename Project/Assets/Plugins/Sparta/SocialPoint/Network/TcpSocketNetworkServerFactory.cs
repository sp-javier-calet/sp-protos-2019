using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class TcpSocketNetworkServerFactory : INetworkServerFactory
    {
        readonly TcpSocketNetworkInstaller.SettingsData _settings;
        readonly IUpdateScheduler _updateScheduler;
        readonly List<INetworkServerDelegate> _delegates;

        public TcpSocketNetworkServerFactory(TcpSocketNetworkInstaller.SettingsData settings, IUpdateScheduler updateScheduler, List<INetworkServerDelegate> delegates)
        {
            _settings = settings;
            _updateScheduler = updateScheduler;
            _delegates = delegates;
        }

        #region INetworkServerFactory implementation

        INetworkServer INetworkServerFactory.Create()
        {
            var server = new TcpSocketNetworkServer(_updateScheduler,
                _settings.Config.ServerAddress, _settings.Config.ServerPort);
            SetupServer(server);

            return server;
        }

        #endregion

        void SetupServer(INetworkServer server)
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                server.AddDelegate(_delegates[i]);
            }
        }
    }
}

