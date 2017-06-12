using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.IO;
using ExitGames.Client.Photon;
using System.IO;

namespace SocialPoint.Network
{
    public class PhotonNetworkServer : PhotonNetworkBase, INetworkServer
    {
        public PhotonNetworkClient LocalPhotonClient;

        public bool HasLocalPhotonClient
        {
            get
            {
                return LocalPhotonClient != null;
            }
        }
        
        List<INetworkServerDelegate> _delegates = new List<INetworkServerDelegate>();
        INetworkMessageReceiver _receiver;

        void INetworkServer.Start()
        {
            DoConnect();
        }

        public void Stop()
        {
            DoDisconnect();
        }

        public void Fail(Error err)
        {
            if(!Running)
            {
                return;
            }
            PhotonNetwork.RaiseEvent(PhotonMsgType.Fail, err.ToString(), true, null);
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
            return PhotonNetwork.ServerTimestamp;
        }

        public bool Running
        {
            get
            {
                return State == ConnState.Connected;
            }
        }

        public byte ClientId
        {
            get
            {
                return GetClientId(PhotonNetwork.player);
            }
        }

        public string Id
        {
            get
            {
                if(PhotonNetwork.room == null)
                {
                    return null;
                }
                return PhotonNetwork.room.name;
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

        bool SetServerPlayer()
        {
            if(PhotonNetwork.room.customProperties.ContainsKey(ServerIdRoomProperty))
            {
                return false;
            }
            var props = new Hashtable {
                { ServerIdRoomProperty, PhotonNetwork.player.ID }
            };
            PhotonNetwork.room.SetCustomProperties(props);
            return true;
        }

        public void OnPhotonPlayerConnected(PhotonPlayer player)
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
            if(!SetServerPlayer())
            {
                OnNetworkError(new Error("There is already a server in the room."));
                Stop();
                return;
            }
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnServerStarted();
            }
            var players = PhotonNetwork.otherPlayers;
            for(var i = 0; i < players.Length; i++)
            {
                OnPhotonPlayerConnected(players[i]);
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

        public override void SendNetworkMessage(NetworkMessageData info, byte[] data)
        {
            var cdata = HttpEncoding.Encode(data, HttpEncoding.DefaultBodyCompression);

            var options = new RaiseEventOptions();

            var reliable = PhotonNetwork.PhotonServerSettings.Protocol == ConnectionProtocol.Tcp && !info.Unreliable;
            if(info.ClientId != 0)
            {
                var player = GetPlayer(info.ClientId);
                if(player == null)
                {
                    return;
                }

                options.TargetActors = new int[]{ player.ID };
                PhotonNetwork.RaiseEvent(info.MessageType, cdata, reliable, options);
            }
            else
            {
                options.Receivers = HasLocalPhotonClient ? ReceiverGroup.All : ReceiverGroup.Others;
                PhotonNetwork.RaiseEvent(info.MessageType, cdata, reliable, options);
            }

            Config.CustomPhotonConfig.RegisterOnGoingCommand();
        }

        protected override void ProcessOnEventReceived(byte eventcode, object content, int senderid)
        {
            var cdata = HttpEncoding.Decode((byte[])content, HttpEncoding.DefaultBodyCompression);
            
            byte clientId = GetClientId(GetPlayer((byte)senderid));
            var info = new NetworkMessageData {
                MessageType = eventcode,
                ClientId = clientId
            };
            var stream = new MemoryStream(cdata);
            var reader = new SystemBinaryReader(stream);

            OnMessageReceived(info, reader);
        }
    }
}
