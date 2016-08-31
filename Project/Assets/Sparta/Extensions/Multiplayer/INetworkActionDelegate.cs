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

        public override bool Equals(System.Object obj)
        {
            var dlg = obj as NetworkActionDelegate<T>;
            if(dlg != null)
            {
                return this == dlg;
            }
            var cb = obj as Action<T, NetworkScene>;
            if(cb != null)
            {
                return _callback == cb;
            }
            return false;
        }

        public bool Equals(NetworkActionDelegate<T> dlg)
        {             
            return this == dlg;
        }

        public override int GetHashCode()
        {
            return _callback.GetHashCode();
        }

        public static bool operator ==(NetworkActionDelegate<T> a, NetworkActionDelegate<T> b)
        {
            var na = (object)a == null;
            var nb = (object)b == null;
            if(na && nb)
            {
                return true;
            }
            else if(na || nb)
            {
                return false;
            }
            return a._callback == b._callback;
        }

        public static bool operator !=(NetworkActionDelegate<T> a, NetworkActionDelegate<T> b)
        {
            return !(a == b);
        }
    }

    public interface IAppliableSceneAction
    {
        void Apply(NetworkScene scene);
    }
}
