using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.IO;
using ExitGames.Client.Photon;
using System.IO;
using SocialPoint.Dependency;

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
            PhotonNetwork.RaiseEvent(PhotonMsgType.Fail, err.ToString(), true, new RaiseEventOptions{Receivers = ReceiverGroup.All});
            if(HasLocalPhotonClient)
            {
                LocalPhotonClient.OnNetworkError(err);
            }
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

        public bool LatencySupported
        {
            get
            {
                return true;
            }
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
                return PhotonNetwork.room == null ? null : PhotonNetwork.room.Name;
            }
        }

        public const string ServerIdRoomProperty = "server";

        public static int PhotonPlayerId
        {
            get
            {
                var room = PhotonNetwork.room;
                object serverId = 0;
                if(room != null && room.CustomProperties.TryGetValue(ServerIdRoomProperty, out serverId))
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
            if(PhotonNetwork.room.CustomProperties.ContainsKey(ServerIdRoomProperty))
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

        protected override void DoConnect()
        {
            if(HasLocalPhotonClient)
            {
                _state = ConnState.Connecting;
                OnJoinedRoom();
                OnPhotonPlayerConnected(PhotonNetwork.player);
            }
            else
            {
                base.DoConnect();
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
            base.OnDisconnected();
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnServerStopped();
            }
        }

        internal override void OnNetworkError(Error err)
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

        protected override bool IsRecoverableDisconnectCause(DisconnectCause cause)
        {
            return false;
        }

        public override void SendNetworkMessage(NetworkMessageData info, byte[] data)
        {
            var cdata = HttpEncoding.Encode(data, HttpEncoding.LZ4);

            var options = new RaiseEventOptions();

            var reliable = PhotonNetwork.PhotonServerSettings.Protocol == ConnectionProtocol.Tcp && !info.Unreliable;
            if(info.ClientIds != null && info.ClientIds.Count > 0)
            {
                int max = info.ClientIds.Count;
                options.TargetActors = new int[max];
                for(int i = 0; i < max; ++i)
                {
                    var player = GetPlayer(info.ClientIds[i]);
                    if(player == null)
                    {
                        options.TargetActors[i] = 0;
                        return;
                    }

                    options.TargetActors[i] = player.ID;
                }

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
            var cdata = HttpEncoding.Decode((byte[])content, HttpEncoding.LZ4);
            
            byte clientId = GetClientId(GetPlayer((byte)senderid));
            var info = new NetworkMessageData {
                MessageType = eventcode,
                ClientIds = new List<byte>(){ clientId }
            };
            var stream = new MemoryStream(cdata);
            var reader = new SystemBinaryReader(stream);

            OnMessageReceived(info, reader);
        }
    }

    public class PhotonNetworkServerFactory : INetworkServerFactory
    {
        readonly PhotonNetworkInstaller.SettingsData _settings;
        readonly bool _setDelegates;

        public PhotonNetworkServerFactory(PhotonNetworkInstaller.SettingsData settings, bool setDelegates = true)
        {
            _settings = settings;
            _setDelegates = setDelegates;
        }
        
        #region INetworkServerFactory implementation

        INetworkServer INetworkServerFactory.Create()
        {
            var transform = Services.Instance.Resolve<UnityEngine.Transform>();
            var server = transform.gameObject.AddComponent<PhotonNetworkServer>();

            SetupServer(server);

            return server;
        }

        #endregion

        void SetupServer(PhotonNetworkServer server)
        {
            server.Config = _settings.Config;

            if(_setDelegates)
            {
                var dlgs = Services.Instance.ResolveList<INetworkServerDelegate>();
                for(var i = 0; i < dlgs.Count; i++)
                {
                    server.AddDelegate(dlgs[i]);
                }
            }
        }
    }
}
