using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class TcpSocketNetworkClientFactory : INetworkClientFactory
    {
        readonly TcpSocketNetworkInstaller.SettingsData _settings;
        readonly IUpdateScheduler _updateScheduler;
        readonly List<INetworkClientDelegate> _delegates;

        public TcpSocketNetworkClientFactory(TcpSocketNetworkInstaller.SettingsData settings, IUpdateScheduler updateScheduler, List<INetworkClientDelegate> delegates)
        {
            _settings = settings;
            _updateScheduler = updateScheduler;
            _delegates = delegates;
        }

        #region INetworkClientFactory implementation

        INetworkClient INetworkClientFactory.Create()
        {
            var client = new TcpSocketNetworkClient(_updateScheduler,
                _settings.Config.ServerAddress, _settings.Config.ServerPort);
            SetupClient(client);

            return client;
        }

        #endregion

        void SetupClient(INetworkClient client)
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                client.AddDelegate(_delegates[i]);
            }
        }
    }
}