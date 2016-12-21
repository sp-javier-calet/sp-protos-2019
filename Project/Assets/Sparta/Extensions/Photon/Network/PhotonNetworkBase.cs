using SocialPoint.Base;
using SocialPoint.Utils;
using SocialPoint.IO;
using System;
using System.IO;

namespace SocialPoint.Network
{
    [Serializable]
    public class PhotonNetworkRoomConfig
    {
        public const bool DefaultIsVisible = true;
        public const bool DefaultIsOpen = true;
        public const byte DefaultMaxPlayers = 2;
        public const int DefaultPlayerTtl = 0;
        public const int DefaultEmptyRoomTtl = 0;
        public const bool DefaultCleanupCache = true;
        public const bool DefaultPublishUserId = false;

        public bool IsVisible = DefaultIsVisible;
        public bool IsOpen = DefaultIsOpen;
        public byte MaxPlayers = DefaultMaxPlayers;
        public int PlayerTtl = DefaultPlayerTtl;
        public int EmptyRoomTtl = DefaultEmptyRoomTtl;
        public bool CleanupCache = DefaultCleanupCache;
        public bool PublishUserId = DefaultPublishUserId;
        public string[] CustomProperties;
        public string[] CustomLobbyProperties;
        public string[] Plugins;

        public RoomOptions ToPhoton()
        {
            return new RoomOptions {
                IsVisible = IsVisible,
                IsOpen = IsOpen,
                MaxPlayers = MaxPlayers,
                PlayerTtl = PlayerTtl,
                EmptyRoomTtl = EmptyRoomTtl,
                CleanupCacheOnLeave = CleanupCache,
                CustomRoomPropertiesForLobby = CustomLobbyProperties,
                Plugins = Plugins,
                PublishUserId = PublishUserId
            };
        }
    }

    [Serializable]
    public class PhotonNetworkConfig
    {
        public const bool DefaultCreateRoom = true;

        public string GameVersion;
        public string RoomName;
        public bool CreateRoom = DefaultCreateRoom;
        public CustomPhotonConfig CustomPhotonConfig = new CustomPhotonConfig();
        public PhotonNetworkRoomConfig RoomOptions = new PhotonNetworkRoomConfig();
        public CloudRegionCode ForceRegion = CloudRegionCode.none;
        public string ForceAppId;
        public string ForceServer;
    }

    public abstract class PhotonNetworkBase : Photon.MonoBehaviour, IDisposable
    {
        public PhotonNetworkConfig Config;

        protected enum ConnState
        {
            Disconnected,
            Connecting,
            Connected,
            Disconnecting
        }

        ConnState _state = ConnState.Disconnected;
        protected ConnState State
        {
            get
            {
                return _state;
            }
        }


        const int ConnectionError = 1;
        const int CreateRoomError = 2;
        const int CustomAuthError = 3;

        [Obsolete("Use the Config property")]
        public void Init(PhotonNetworkConfig config)
        {
            Config = config;
        }

        void Awake()
        {
            if(Config == null)
            {
                Config = new PhotonNetworkConfig();
            }
        }

        void Update()
        {
            Config.CustomPhotonConfig.SendOutgoingCommands();
        }

        protected void DoConnect()
        {
            if(PhotonNetwork.connectionState != ConnectionState.Disconnected)
            {
                return;
            }
            _state = ConnState.Connecting;
            Config.CustomPhotonConfig.SetConfigBeforeConnection();
            if(!string.IsNullOrEmpty(Config.ForceServer) || !string.IsNullOrEmpty(Config.ForceAppId))
            {
                string addr = null;
                int port = 0;
                string appId = Config.ForceAppId;
                StringUtils.ParseServer(Config.ForceServer, out addr, out port);
                if(string.IsNullOrEmpty(addr))
                {
                    addr = PhotonNetwork.PhotonServerSettings.ServerAddress;
                }
                if(port == 0)
                {
                    port = PhotonNetwork.PhotonServerSettings.ServerPort;
                }
                if(string.IsNullOrEmpty(appId))
                {
                    appId = PhotonNetwork.PhotonServerSettings.AppID;
                }
                PhotonNetwork.ConnectToMaster(addr, port, appId, Config.GameVersion);
            }
            if(Config.ForceRegion != CloudRegionCode.none)
            {
                PhotonNetwork.ConnectToRegion(Config.ForceRegion, Config.GameVersion);
            }
            else
            {
                PhotonNetwork.ConnectUsingSettings(Config.GameVersion);
            }
        }

        protected void DoDisconnect()
        {
            if(PhotonNetwork.connectionState != ConnectionState.Connected)
            {
                return;
            }
            _state = ConnState.Disconnecting;
            PhotonNetwork.Disconnect();
        }

        public void Dispose()
        {
            DoDisconnect();
            Destroy(this);
        }

        RoomOptions PhotonRoomOptions
        {
            get
            {
                return Config.RoomOptions == null ? null : Config.RoomOptions.ToPhoton();
            }
        }

        void JoinOrCreateRoom()
        {
            if(!Config.CreateRoom)
            {
                PhotonNetwork.JoinRoom(Config.RoomName);
            }
            else if(string.IsNullOrEmpty(Config.RoomName))
            {
                PhotonNetwork.CreateRoom(Config.RoomName, PhotonRoomOptions, null);
            }
            else
            {
                PhotonNetwork.JoinOrCreateRoom(Config.RoomName, PhotonRoomOptions, null);
            }
        }

        abstract protected void OnNetworkError(Error err);

        abstract protected void OnConnected();

        abstract protected void OnDisconnected();

        abstract protected void OnMessageReceived(NetworkMessageData data, IReader reader);

        #region photon callbacks

        void OnConnectedToMaster()
        {
            if(State != ConnState.Connecting)
            {
                return;
            }
            if(!PhotonNetwork.autoJoinLobby)
            {
                PhotonNetwork.JoinLobby();
            }
        }

        void OnJoinedLobby()
        {
            if(State != ConnState.Connecting)
            {
                return;
            }
            if(string.IsNullOrEmpty(Config.RoomName))
            {
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                JoinOrCreateRoom();
            }
        }

        void OnPhotonRandomJoinFailed()
        {
            if(State != ConnState.Connecting)
            {
                return;
            }
            JoinOrCreateRoom();
        }

        public void OnPhotonJoinRoomFailed(object[] codeAndMsg)
        {
            if(State != ConnState.Connecting)
            {
                return;
            }
            var err = new Error(ConnectionError, "Failed to join: " + StringUtils.Join(codeAndMsg, " "));
            OnNetworkError(err);
        }

        public void OnFailedToConnectToPhoton(DisconnectCause cause)
        {
            if(State != ConnState.Connecting)
            {
                return;
            }
            var err = new Error(ConnectionError, "Failed to connect: " + cause);
            OnNetworkError(err);
        }

        public void OnPhotonCreateRoomFailed(object[] codeAndMsg)
        {
            if(State != ConnState.Connecting)
            {
                return;
            }
            var err = new Error(CreateRoomError, "Failed to create room: " + StringUtils.Join(codeAndMsg, " "));
            OnNetworkError(err);
            DoDisconnect();
            OnDisconnected();
        }

        void OnJoinedRoom()
        {
            if(State != ConnState.Connecting)
            {
                return;
            }
            _state = ConnState.Connected;
            PhotonNetwork.OnEventCall += OnEventReceived;
            Config.CustomPhotonConfig.SetConfigOnJoinedRoom();
            OnConnected();
        }

        void OnLeftRoom()
        {
            if(State != ConnState.Connected)
            {
                return;
            }
            PhotonNetwork.Disconnect();
        }

        void OnDisconnectedFromPhoton()
        {
            if(State == ConnState.Disconnected)
            {
                return;
            }
            PhotonNetwork.OnEventCall -= OnEventReceived;
            _state = ConnState.Disconnected;
            Config.CustomPhotonConfig.RestorePhotonConfig();
            OnDisconnected();
        }

        void OnConnectionFail(DisconnectCause cause)
        {
            if(State != ConnState.Connected)
            {
                return;
            }
            var err = new Error(ConnectionError, "Failed to connect: " + cause);
            _state = ConnState.Disconnected;
            OnNetworkError(err);
            OnDisconnected();
        }

        void OnCustomAuthenticationFailed(string debugMessage)
        {
            if(State != ConnState.Connected)
            {
                return;
            }
            var err = new Error(CustomAuthError, "Custom Authentication failed: " + debugMessage);
            _state = ConnState.Disconnected;
            OnNetworkError(err);
            OnDisconnected();
        }

        #endregion

        protected static byte GetClientId(PhotonPlayer player)
        {
            if(player == null)
            {
                return 0;
            }
            return (byte)player.ID;
        }

        protected static PhotonPlayer GetPlayer(byte clientId)
        {
            var players = PhotonNetwork.otherPlayers;
            for(var i = 0; i < players.Length; i++)
            {
                var player = players[i];
                if(player.ID == clientId)
                {
                    return player;
                }
            }
            return null;
        }

        public INetworkMessage CreateMessage(NetworkMessageData info)
        {
            return new PhotonNetworkMessage(info, this);
        }

        public void SendNetworkMessage(NetworkMessageData info, byte[] data)
        {
            var options = new RaiseEventOptions();

            var serverId = PhotonNetworkServer.PhotonPlayerId;
            if(PhotonNetwork.player.ID != serverId)
            {
                // clients always send to server
                options.TargetActors = new int[]{ serverId };
            }
            else if(info.ClientId != 0)
            {
                var player = GetPlayer(info.ClientId);
                if(player == null)
                {
                    return;
                }
                options.TargetActors = new int[]{ player.ID };
            }
            PhotonNetwork.RaiseEvent(info.MessageType, data, !info.Unreliable, options);
            Config.CustomPhotonConfig.RegisterOnGoingCommand();
        }

        void OnEventReceived(byte eventcode, object content, int senderid)
        {
            if(eventcode == EventCode.ErrorInfo || eventcode == PhotonMsgType.Fail)
            {
                var err = Error.FromString((string)content);
                OnNetworkError(err);
                DoDisconnect();
                return;
            }

            byte clientId = 0;
            var serverId = PhotonNetworkServer.PhotonPlayerId;
            if(senderid != serverId)
            {
                clientId = GetClientId(GetPlayer((byte)senderid));
            }
            var info = new NetworkMessageData {
                MessageType = eventcode,
                ClientId = clientId
            };
            var stream = new MemoryStream((byte[])content);
            var reader = new SystemBinaryReader(stream);
            OnMessageReceived(info, reader);
        }
    }
}
