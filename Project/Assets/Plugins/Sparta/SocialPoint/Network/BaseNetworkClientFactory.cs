using System.Collections.Generic;

namespace SocialPoint.Network
{
    public abstract class BaseNetworkClientFactory<T> : INetworkClientFactory where T : INetworkClient
    {
        readonly List<INetworkClientDelegate> _delegates;

        public BaseNetworkClientFactory(List<INetworkClientDelegate> delegates = null)
        {
            _delegates = delegates;
        }

        protected abstract T DoCreate();

        public T Create()
        {
            var client = DoCreate();
            SetupDelegates(client);
            return client;
        }

        INetworkClient INetworkClientFactory.Create()
        {
            return Create();
        }

        protected void SetupDelegates(INetworkClient client)
        {
            if(_delegates != null)
            {
                for(var i = 0; i < _delegates.Count; i++)
                {
                    client.AddDelegate(_delegates[i]);
                }
            }
        }
    }
}