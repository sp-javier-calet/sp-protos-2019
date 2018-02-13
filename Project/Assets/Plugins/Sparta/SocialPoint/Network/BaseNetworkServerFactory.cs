using System.Collections.Generic;

namespace SocialPoint.Network
{
    public class BaseNetworkServerFactory
    {
        readonly List<INetworkServerDelegate> _delegates;
        protected INetworkServer _server;

        public BaseNetworkServerFactory(List<INetworkServerDelegate> delegates = null)
        {
            _delegates = delegates;
        }

        protected T Create<T>(T server) where T : INetworkServer
        {
            _server = server;
            SetupDelegates();

            return server;
        }

        void SetupDelegates()
        {
            if(_delegates != null)
            {
                for(var i = 0; i < _delegates.Count; i++)
                {
                    _server.AddDelegate(_delegates[i]);
                }
            }
        }
    }
}

