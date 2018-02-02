using System;
using SocialPoint.Network;

namespace SocialPoint.Sockets
{
    public class NetworkMatchDelegateFactory : INetworkMatchDelegateFactory
    {
        public object Create(string matchId, INetworkMessageSender sender)
        {
            return new NetworkServerMatchHandler(matchId, sender);
        }
    }
}