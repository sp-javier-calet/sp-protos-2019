using System;
using SocialPoint.Base;


namespace SocialPoint.WAMP
{
    internal class WAMPConnectionEventDispatcher
    {
        public event Action OnClientConnected;
        public event Action OnClientDisconnected;
        public event Action<Error> OnNetworkError;


    }
}
