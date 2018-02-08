using System.Collections.Generic;

namespace SocialPoint.Network
{
    public class PhotonNetworkClientFactory : INetworkClientFactory
    {
        readonly PhotonNetworkInstaller.SettingsData _settings;
        readonly UnityEngine.Transform _transform;
        readonly List<INetworkClientDelegate> _delegates;

        public PhotonNetworkClientFactory(PhotonNetworkInstaller.SettingsData settings, UnityEngine.Transform transform, List<INetworkClientDelegate> delegates)
        {
            _settings = settings;
            _transform = transform;
            _delegates = delegates;
        }

        #region INetworkClientFactory implementation

        INetworkClient INetworkClientFactory.Create()
        {
            var client = _transform.gameObject.AddComponent<PhotonNetworkClient>();

            SetupClient(client);

            return client;
        }

        #endregion

        void SetupClient(PhotonNetworkClient client)
        {
            client.Config = _settings.Config;

            for(var i = 0; i < _delegates.Count; i++)
            {
                client.AddDelegate(_delegates[i]);
            }
        }
    }
}