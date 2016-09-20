using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.IO;

namespace SocialPoint.Network
{
    class PhotonNetworkServer : PhotonNetworkBase, INetworkServer
    {
        List<INetworkServerDelegate> _delegates = new List<INetworkServerDelegate>();
        INetworkMessageReceiver _receiver;

        public void Start()
        {
            DoConnect();
        }

        public void Stop()
        {
            DoDisconnect();
        }

        public void AddDelegate(INetworkServerDelegate dlg)
        {
            _delegates.Add(dlg);
        }

        public void RemoveDelegate(INetworkServerDelegate dlg)
        {
            _delegates.Remove(dlg);
        }

        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        public int GetTimestamp()
        {
            return (int)PhotonNetwork.time;
        }

        public bool Running
        {
            get
            {
                return PhotonNetwork.connected;
            }
        }

        public const string ServerIdRoomProperty = "server";

        public static int PhotonPlayerId
        {
            get
            {
                var room = PhotonNetwork.room;
                object serverId = 0;
                if(room != null && room.customProperties.TryGetValue(ServerIdRoomProperty, out serverId))
                {
                    if(serverId is int)
                    {
                        return (int)serverId;
                    }
                }
                return 0;
            }
        }

        void SetServerPlayer()
        {
            var props = new ExitGames.Client.Photon.Hashtable{
                { ServerIdRoomProperty, PhotonNetwork.player.ID }};
            PhotonNetwork.room.SetCustomProperties(props);
        }

        void OnPhotonPlayerConnected(PhotonPlayer player)
        {
            var clientId = GetClientId(player);
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientConnected(clientId);
            }
        }

        void OnPhotonPlayerDisconnected(PhotonPlayer player)
        {
            var clientId = GetClientId(player);
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientDisconnected(clientId);
            }
        }

        protected override void OnConnected()
        {
            SetServerPlayer();
            var players = PhotonNetwork.otherPlayers;
            for(var i = 0; i < players.Length; i++)
            {
                OnPhotonPlayerConnected(players[i]);
            }
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnServerStarted();
            }
        }

        protected override void OnDisconnected()
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnServerStopped();
            }
        }

        protected override void OnNetworkError(Error err)
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnNetworkError(err);
            }
        }

        protected override void OnMessageReceived(NetworkMessageData data, IReader reader)
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnMessageReceived(data);
            }
            if(_receiver != null)
            {
                _receiver.OnMessageReceived(data, reader);
            }
        }
    }
}
