using System;
using System.Collections.Generic;

namespace SocialPoint.Network
{
    public class PhotonNetworkClientFactory : BaseNetworkClientFactory<PhotonNetworkClient>
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

        protected override PhotonNetworkClient DoCreate()
        {
            var photon = _transform.GetComponent<PhotonNetworkBase>();
            if(photon != null)
            {
                throw new Exception("There is already a Photon network object instantiated. Photon cannot have more than one connection!");
            }

            var client = _transform.gameObject.AddComponent<PhotonNetworkClient>();
            client.Config = _settings.Config;

            return client;
        }
    }
}