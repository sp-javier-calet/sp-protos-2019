using System;
using System.Collections.Generic;

namespace SocialPoint.Network
{
    public class PhotonNetworkServerFactory : BaseNetworkServerFactory<PhotonNetworkServer>
    {
        readonly PhotonNetworkInstaller.SettingsData _settings;
        readonly UnityEngine.Transform _transform;

        public PhotonNetworkServerFactory(
            PhotonNetworkInstaller.SettingsData settings,
            UnityEngine.Transform transform,
            List<INetworkServerDelegate> delegates = null) : base(delegates)
        {
            _settings = settings;
            _transform = transform;
        }

        protected override PhotonNetworkServer DoCreate()
        {
            var photon = _transform.GetComponent<PhotonNetworkBase>();
            if(photon != null)
            {
                throw new Exception("There is already a Photon network object instantiated. Photon cannot have more than one connection!");
            }

            var server = _transform.gameObject.AddComponent<PhotonNetworkServer>();
            server.Config = _settings.Config;

            return server;
        }
    }
}

