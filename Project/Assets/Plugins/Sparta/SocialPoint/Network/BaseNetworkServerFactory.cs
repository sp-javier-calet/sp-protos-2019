using System.Collections.Generic;

namespace SocialPoint.Network
{
    public abstract class BaseNetworkServerFactory<T> : INetworkServerFactory where T : INetworkServer
    {
        readonly List<INetworkServerDelegate> _delegates;

        public BaseNetworkServerFactory(List<INetworkServerDelegate> delegates = null)
        {
            _delegates = delegates;
        }

        protected abstract T DoCreate();

        public T Create()
        {
            var server = DoCreate();
            SetupDelegates(server);
            return server;
        }

        INetworkServer INetworkServerFactory.Create()
        {
            return Create();
        }

        protected void SetupDelegates(INetworkServer server)
        {
            if(_delegates != null)
            {
                for(var i = 0; i < _delegates.Count; i++)
                {
                    server.AddDelegate(_delegates[i]);
                }
            }
        }
    }
}