using System.Collections.Generic;

namespace SocialPoint.Network
{
    public class UnetNetworkClientFactory : INetworkClientFactory
    {
        readonly UnetNetworkInstaller.SettingsData _settings;
        readonly List<INetworkClientDelegate> _delegates;

        public UnetNetworkClientFactory(UnetNetworkInstaller.SettingsData settings, List<INetworkClientDelegate> delegates)
        {
            _settings = settings;
            _delegates = delegates;
        }
        #region INetworkClientFactory implementation

        INetworkClient INetworkClientFactory.Create()
        {
            return Create();
        }

        #endregion

        public UnetNetworkClient Create()
        {
            var client = new UnetNetworkClient(_settings.Config.ServerAddress, _settings.Config.ServerPort);
            SetupClient(client);

            return client;
        }

        void SetupClient(INetworkClient client)
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                client.AddDelegate(_delegates[i]);
            }
        }
    }
}