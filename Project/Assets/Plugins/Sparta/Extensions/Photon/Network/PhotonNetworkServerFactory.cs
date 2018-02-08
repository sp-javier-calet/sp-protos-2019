using System.Collections.Generic;

namespace SocialPoint.Network
{
    public class PhotonNetworkServerFactory : INetworkServerFactory
    {
        readonly PhotonNetworkInstaller.SettingsData _settings;
        readonly UnityEngine.Transform _transform;
        readonly List<INetworkServerDelegate> _delegates;
        readonly bool _setDelegates;

        public PhotonNetworkServerFactory(PhotonNetworkInstaller.SettingsData settings, UnityEngine.Transform transform, List<INetworkServerDelegate> delegates, bool setDelegates = true)
        {
            _settings = settings;
            _transform = transform;
            _delegates = delegates;
            _setDelegates = setDelegates;
        }

        #region INetworkServerFactory implementation

        INetworkServer INetworkServerFactory.Create()
        {
            var server = _transform.gameObject.AddComponent<PhotonNetworkServer>();

            SetupServer(server);

            return server;
        }

        #endregion

        void SetupServer(PhotonNetworkServer server)
        {
            server.Config = _settings.Config;

            if(_setDelegates)
            {
                for(var i = 0; i < _delegates.Count; i++)
                {
                    server.AddDelegate(_delegates[i]);
                }
            }
        }
    }
}

