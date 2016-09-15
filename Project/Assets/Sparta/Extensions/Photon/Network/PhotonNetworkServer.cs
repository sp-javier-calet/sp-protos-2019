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
            return 0;
        }

        public bool Running
        {
            get
            {
                return PhotonNetwork.connected;
            }
        }

        void OnPhotonPlayerConnected(PhotonPlayer player)
        {
            var clientId = GetPlayerClientId(player);
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientConnected(clientId);
            }
        }

        void OnPhotonPlayerDisconnected(PhotonPlayer player)
        {
            var clientId = GetPlayerClientId(player);
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientDisconnected(clientId);
            }
        }

        protected override void OnConnected()
        {
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
