using System;
using System.Collections;
using SocialPoint.IO;

namespace SocialPoint.Multiplayer
{
    public interface INetworkActionDelegate
    {
        /// <summary>
        /// Implementation must apply the action to the scene object.
        /// </summary>
        /// <param name="action">Action. Should be cast to the expected type.</param>
        /// <param name="scene">Scene.</param>
        void ApplyAction(object action, NetworkScene scene);
    }

    public class NetworkActionDelegate<T> : INetworkActionDelegate
    {
        Action<T, NetworkScene> _callback;

        public NetworkActionDelegate(Action<T, NetworkScene> callback)
        {
            _callback = callback;
        }

        public void ApplyAction(object data, NetworkScene scene)
        {
            if(_callback != null && data is T)
            {
                _callback((T)data, scene);
            }
        }
    }
}
