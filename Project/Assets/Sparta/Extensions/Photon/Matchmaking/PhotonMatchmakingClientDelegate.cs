using SocialPoint.Network;
using SocialPoint.Base;
using System;

namespace SocialPoint.Matchmaking
{
    public class PhotonMatchmakingClientDelegate : IMatchmakingClientDelegate, INetworkClientDelegate, IDisposable
    {
        IMatchmakingClient _matchmaking;
        PhotonNetworkClient _network;

        const string RegionAttrKey = "region";
        const string ServerAttrKey = "addr";
        const string AppIdAttrKey = "app_id";

        public PhotonMatchmakingClientDelegate(PhotonNetworkClient network, IMatchmakingClient matchmaking)
        {
            _network = network;
            _matchmaking = matchmaking;
            _network.AddDelegate(this);
        }

        public void Dispose()
        {
            _network.RemoveDelegate(this);
        }

        public void OnStart()
        {
        }

        public void OnSearchOpponent()
        {
        }

        public void OnWaiting(int waitTime)
        {
        }

        public void OnMatched(Match match)
        {
            _network.Config.RoomName = match.Id;
            _network.Config.CreateRoom = !match.Running;
            var server = match.ServerInfo.AsDic;
            if(server.ContainsKey(RegionAttrKey))
            {
                try
                {
                    var region = server.GetValue(RegionAttrKey).ToString();
                    _network.Config.ForceRegion = (CloudRegionCode)Enum.Parse(typeof(CloudRegionCode), region, true);
                }
                catch(Exception)
                {
                }
            }
            if(server.ContainsKey(ServerAttrKey))
            {
                _network.Config.ForceServer = server.GetValue(ServerAttrKey).ToString();
            }
            if(server.ContainsKey(AppIdAttrKey))
            {
                _network.Config.ForceAppId = server.GetValue(AppIdAttrKey).ToString();
            }
        }

        public void OnStopped(bool successful)
        {
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
