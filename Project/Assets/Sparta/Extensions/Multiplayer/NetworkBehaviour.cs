using SocialPoint.Pooling;

namespace SocialPoint.Multiplayer
{
    public abstract class NetworkBehaviour : INetworkBehaviour
    {
        NetworkGameObject _go;

        NetworkGameObject INetworkBehaviour.GameObject
        {
            set
            {
                _go = value;
            }
        }

        public NetworkGameObject GameObject
        {
            get
            {
                return _go;
            }
        }

        protected void Copy(NetworkBehaviour other)
        {
            if(other != null)
            {
                _go = other._go;
            }
        }

        void INetworkBehaviour.OnAwake()
        {
            OnAwake();
        }

        protected virtual void OnAwake()
        {
        }

        void INetworkBehaviour.OnStart()
        {
            OnStart();
        }

        protected virtual void OnStart()
        {
        }

        void INetworkBehaviour.OnDestroy()
        {
            OnDestroy();
        }

        protected virtual void OnDestroy()
        {
        }

        void INetworkBehaviour.Update(float dt)
        {
            Update(dt);
        }

        protected virtual void Update(float dt)
        {
        }

        public abstract object Clone();

        void INetworkBehaviour.Dispose()
        {
            Dispose();
            _go = null;
        }

        protected virtual void Dispose()
        {
            ObjectPool.Return(this);
        }
    }
}
