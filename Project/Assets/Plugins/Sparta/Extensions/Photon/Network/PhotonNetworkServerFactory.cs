using System;
using System.Collections.Generic;

namespace SocialPoint.Network
{
    public class PhotonNetworkServerFactory : BaseNetworkServerFactory, INetworkServerFactory
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

        #region INetworkServerFactory implementation

        INetworkServer INetworkServerFactory.Create()
        {
            return Create();
        }

        #endregion

        public PhotonNetworkServer Create()
        {
            var photon = _transform.GetComponent<PhotonNetworkBase>();
            if(photon != null)
            {
                throw new Exception("There is already a Photon network object instantiated. Photon cannot have more than one connection!");
            }

            var server = Create<PhotonNetworkServer>(_transform.gameObject.AddComponent<PhotonNetworkServer>());
            server.Config = _settings.Config;

            return server;
        }
    }
}

