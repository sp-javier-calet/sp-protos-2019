using SocialPoint.Utils;
using System;

namespace SocialPoint.Multiplayer
{
    public class NetworkSceneBehaviour : INetworkSceneBehaviour
    {
        NetworkScene _scene;

        NetworkScene INetworkSceneBehaviour.Scene
        {
            set
            {
                _scene = value;
            }
        }

        public NetworkScene Scene
        {
            get
            {
                return _scene;
            }
        }

        void INetworkSceneBehaviour.OnStart()
        {
            OnStart();
        }

        protected virtual void OnStart()
        {
        }

        void INetworkSceneBehaviour.OnDestroy()
        {
            OnDestroy();
        }

        protected virtual void OnDestroy()
        {
        }

        void IDeltaUpdateable.Update(float dt)
        {
            Update(dt);
        }

        protected virtual void Update(float dt)
        {
        }

        void INetworkSceneBehaviour.OnInstantiateObject(NetworkGameObject go)
        {
            OnInstantiateObject(go);
        }

        protected virtual void OnInstantiateObject(NetworkGameObject go)
        {
        }

        void INetworkSceneBehaviour.OnDestroyObject(int id)
        {
            OnDestroyObject(id);
        }

        protected virtual void OnDestroyObject(int id)
        {
        }

        void IDisposable.Dispose()
        {
            Dispose();
        }

        public virtual void Dispose()
        {
            if (_scene != null)
            {
                _scene.Context.Pool.Return(this);
                _scene = null;
            }
        }
    }
}
