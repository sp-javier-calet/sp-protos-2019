using SocialPoint.Network;

namespace SocialPoint.Examples.Sockets
{
    public class NetworkMatchDelegateFactory : INetworkMatchDelegateFactory
    {
        public object Create(string matchId, INetworkMessageSender sender)
        {
            return new NetworkServerMatchHandler(matchId, sender);
        }
    }
}