using System;
using System.Collections.Generic;

namespace SocialPoint.Network
{
    public class PhotonNetworkClientFactory : BaseNetworkClientFactory, INetworkClientFactory
    {
        readonly PhotonNetworkInstaller.SettingsData _settings;
        readonly UnityEngine.Transform _transform;

        public PhotonNetworkClientFactory(
            PhotonNetworkInstaller.SettingsData settings,
            UnityEngine.Transform transform,
            List<INetworkClientDelegate> delegates) : base(delegates)
        {
            _settings = settings;
            _transform = transform;
        }

        #region INetworkClientFactory implementation

        INetworkClient INetworkClientFactory.Create()
        {
            return Create();
        }

        #endregion

        public PhotonNetworkClient Create()
        {
            var photon = _transform.GetComponent<PhotonNetworkBase>();
            if(photon != null)
            {
                throw new Exception("There is already a Photon network object instantiated. Photon cannot have more than one connection!");
            }

            var client = Create<PhotonNetworkClient>(_transform.gameObject.AddComponent<PhotonNetworkClient>());
            client.Config = _settings.Config;

            return client;
        }
    }
}