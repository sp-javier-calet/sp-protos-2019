using SocialPoint.Network;
using SocialPoint.Base;
using System;

namespace SocialPoint.Matchmaking
{
    public class PhotonMatchmakingClientDelegate : IMatchmakingClientDelegate, INetworkClientDelegate, IDisposable
    {
        IMatchmakingClientController _matchmaking;
        PhotonNetworkClient _network;

        public PhotonMatchmakingClientDelegate(PhotonNetworkClient network, IMatchmakingClientController matchmaking)
        {
            _network = network;
            _matchmaking = matchmaking;
            _network.AddDelegate(this);
        }

        public void Dispose()
        {
            _network.RemoveDelegate(this);
        }

        public void OnWaiting(int waitTime)
        {
        }

        public void OnMatched(Match match)
        {
            _network.Config.RoomName = match.Id;
        }

        public void OnError(Error err)
        {
        }

        void INetworkClientDelegate.OnClientConnected()
        {
        }

        void INetworkClientDelegate.OnClientDisconnected()
        {
        }

        void INetworkClientDelegate.OnMessageReceived(NetworkMessageData data)
        {
        }

        void INetworkClientDelegate.OnNetworkError(Error err)
        {
            _matchmaking.Clear();
        }
    }
}
