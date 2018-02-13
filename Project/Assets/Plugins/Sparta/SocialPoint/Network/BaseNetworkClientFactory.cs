using System.Collections.Generic;

namespace SocialPoint.Network
{
    public class BaseNetworkClientFactory
    {
        readonly List<INetworkClientDelegate> _delegates;
        protected INetworkClient _client;

        public BaseNetworkClientFactory(List<INetworkClientDelegate> delegates = null)
        {
            _delegates = delegates;
        }

        protected T Create<T>(T client) where T : INetworkClient
        {
            _client = client;
            SetupDelegates();

            return client;
        }

        void SetupDelegates()
        {
            if(_delegates != null)
            {
                for(var i = 0; i < _delegates.Count; i++)
                {
                    _client.AddDelegate(_delegates[i]);
                }
            }
        }
    }
}

