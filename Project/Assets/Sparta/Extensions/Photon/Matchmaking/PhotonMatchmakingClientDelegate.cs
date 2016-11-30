﻿using SocialPoint.Network;
using SocialPoint.Base;
using System;

namespace SocialPoint.Matchmaking
{
    public class PhotonMatchmakingClientDelegate : IMatchmakingClientDelegate, INetworkClientDelegate, IDisposable
    {
        IMatchmakingClient _matchmaking;
        PhotonNetworkClient _network;

        const string RegionAttrKey = "region";
        const string ServerAttrKey = "server";
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

        public void OnWaiting(int waitTime)
        {
        }

        public void OnMatched(Match match)
        {
            _network.Config.RoomName = match.Id;
            var info = match.Info.AsDic;
            if(info.ContainsKey(RegionAttrKey))
            {
                try
                {
                    var region = info.GetValue(RegionAttrKey).ToString();
                    _network.Config.ForceRegion = (CloudRegionCode) Enum.Parse(typeof(CloudRegionCode), region, true);
                }
                catch(Exception)
                {
                }
            }
            if(info.ContainsKey(ServerAttrKey))
            {
                _network.Config.ForceServer = info.GetValue(ServerAttrKey).ToString();
            }
            if(info.ContainsKey(AppIdAttrKey))
            {
                _network.Config.ForceAppId = info.GetValue(AppIdAttrKey).ToString();
            }
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
