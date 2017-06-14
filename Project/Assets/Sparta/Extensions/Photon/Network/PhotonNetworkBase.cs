using System;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.IO;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    [Serializable]
    public class PhotonNetworkRoomConfig
    {
        const string BackendEnvKey = "be_env";

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
        public Dictionary<object, object> CustomProperties;
        public string[] CustomLobbyProperties;
        public string[] Plugins;

        public string BackendEnv
        {
            get
            {
                object url;
                return CustomProperties.TryGetValue(BackendEnvKey, out url) ? url as string : string.Empty;
            }

            set
            {
                CustomProperties[BackendEnvKey] = value;
            }
        }

        public PhotonNetworkRoomConfig()
        {
            CustomProperties = new ExitGames.Client.Photon.Hashtable();
        }

        public RoomOptions ToPhoton()
        {
            var hashtable = new ExitGames.Client.Photon.Hashtable(CustomProperties.Count);
            using(var itr = CustomProperties.GetEnumerator())
            {
                while(itr.MoveNext())
                {
                    hashtable.Add(itr.Current.Key, itr.Current.Value);
                }
            }
            return new RoomOptions {
                IsVisible = IsVisible,
                IsOpen = IsOpen,
                MaxPlayers = MaxPlayers,
                PlayerTtl = PlayerTtl,
                EmptyRoomTtl = EmptyRoomTtl,
                CleanupCacheOnLeave = CleanupCache,
                CustomRoomProperties = hashtable,
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
		public const ExitGames.Client.Photon.ConnectionProtocol DefaultProtocol = ExitGames.Client.Photon.ConnectionProtocol.Tcp;

        public string GameVersion;
        public string RoomName;
        public bool CreateRoom;
        public CustomPhotonConfig CustomPhotonConfig;
        public PhotonNetworkRoomConfig RoomOptions;
        public CloudRegionCode ForceRegion;
        public string ForceAppId;
        public string ForceServer;
		public ExitGames.Client.Photon.ConnectionProtocol Protocol = DefaultProtocol;

        public PhotonNetworkConfig()
        {
        }

        public PhotonNetworkConfig(PhotonNetworkConfig c)
        {
            CreateRoom = c.CreateRoom;

            var cfc = new CustomPhotonConfig();
            cfc.Enabled = c.CustomPhotonConfig.Enabled;
            cfc.MaximumTransferUnit = c.CustomPhotonConfig.MaximumTransferUnit;
            cfc.QuickResendAttempts = c.CustomPhotonConfig.QuickResendAttempts;
            cfc.SentCountAllowance = c.CustomPhotonConfig.SentCountAllowance;
            cfc.UpdateInterval = c.CustomPhotonConfig.UpdateInterval;
            cfc.UpdateIntervalOnSerialize = c.CustomPhotonConfig.UpdateIntervalOnSerialize;
            CustomPhotonConfig = cfc;

            ForceAppId = c.ForceAppId;
            ForceRegion = c.ForceRegion;
            ForceServer = c.ForceServer;
            GameVersion = c.GameVersion;
            RoomName = c.RoomName;

            var rfg = new PhotonNetworkRoomConfig();
            rfg.CleanupCache = c.RoomOptions.CleanupCache;
            rfg.CustomLobbyProperties = (string[])c.RoomOptions.CustomLobbyProperties.Clone();
            rfg.CustomProperties = new Dictionary<object, object>(c.RoomOptions.CustomProperties);
            rfg.EmptyRoomTtl = c.RoomOptions.EmptyRoomTtl;
            rfg.IsOpen = c.RoomOptions.IsOpen;
            rfg.IsVisible = c.RoomOptions.IsVisible;
            rfg.MaxPlayers = c.RoomOptions.MaxPlayers;
            rfg.PlayerTtl = c.RoomOptions.PlayerTtl;
            rfg.Plugins = (string[])c.RoomOptions.Plugins.Clone();
            rfg.PublishUserId = c.RoomOptions.PublishUserId;
            RoomOptions = rfg;
        }
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

        protected ConnState _state = ConnState.Disconnected;

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
        }

        void Update()
        {
            Config.CustomPhotonConfig.SendOutgoingCommands();
        }

        protected virtual void DoConnect()
        {
            if(PhotonNetwork.connectionState != ConnectionState.Disconnected)
            {
                return;
            }
            _state = ConnState.Connecting;
            PhotonNetwork.networkingPeer.TransportProtocol = Config.Protocol;
            Config.CustomPhotonConfig.SetConfigBeforeConnection();
            if(!string.IsNullOrEmpty(Config.ForceServer) && !string.IsNullOrEmpty(Config.ForceAppId))
            {
                string addr;
                int port;
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
                Log.d("[PhotonNetworkBase] ConnectToMaster, addr: " + addr + ", port: " + port + ", appId: " + appId + ", gameversion: " + Config.GameVersion);
                PhotonNetwork.ConnectToMaster(addr, port, appId, Config.GameVersion);
            }
            else if(Config.ForceRegion != CloudRegionCode.none)
            {
                if(!string.IsNullOrEmpty(Config.ForceAppId))
                {
                    PhotonNetwork.PhotonServerSettings.AppID = Config.ForceAppId;
                }
                Log.d("[PhotonNetworkBase] ConnectToRegion, ForceRegion: " + Config.ForceRegion + ", Config.GameVersion: " + Config.GameVersion);
                PhotonNetwork.ConnectToRegion(Config.ForceRegion, Config.GameVersion);
            }
            else
            {
                Log.d("[PhotonNetworkBase] ConnectUsingSettings GameVersion: " + Config.GameVersion);
                PhotonNetwork.ConnectUsingSettings(Config.GameVersion);
            }
        }

        protected void DoDisconnect()
        {
            Log.d("[PhotonNetworkBase] DoDisconnect");
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

        virtual protected void OnDisconnected()
        {
            PhotonNetwork.OnEventCall -= OnEventReceived;
            _state = ConnState.Disconnected;
            Config.CustomPhotonConfig.RestorePhotonConfig();
        }

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
            Log.e("Failed to join: " + StringUtils.Join(codeAndMsg, " "));
            if(State != ConnState.Connecting)
            {
                return;
            }
            var err = new Error(ConnectionError, "Failed to join: " + StringUtils.Join(codeAndMsg, " "));
            OnNetworkError(err);
        }

        public void OnFailedToConnectToPhoton(DisconnectCause cause)
        {
            Log.e("Failed to connect: " + cause);
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

        protected void OnJoinedRoom()
        {
            if(State != ConnState.Connecting)
            {
                return;
            }
            _state = ConnState.Connected;
            Log.e("[PhotonNetworkBase] Registering to OnEventCall");
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
            Log.d("[PhotonNetworkBase] OnDisconnectedFromPhoton");
            if(State == ConnState.Disconnected)
            {
                return;
            }
            OnDisconnected();
        }

        void OnConnectionFail(DisconnectCause cause)
        {
            Log.e("Failed to connect: " + cause);
            if(State != ConnState.Connected)
            {
                return;
            }
            var err = new Error(ConnectionError, "Failed to connect: " + cause);
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
            OnNetworkError(err);
            OnDisconnected();
        }

        #endregion

        protected static byte GetClientId(PhotonPlayer player)
        {
            return player == null ? (byte)0 : (byte)player.ID;
        }

        protected static PhotonPlayer GetPlayer(byte clientId)
        {
            var players = PhotonNetwork.playerList;
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

        public abstract void SendNetworkMessage(NetworkMessageData info, byte[] data);

        void OnEventReceived(byte eventcode, object content, int senderid)
        {
            if(eventcode == EventCode.ErrorInfo || eventcode == PhotonMsgType.Fail)
            {
                var err = Error.FromString((string)content);
                OnNetworkError(err);
                DoDisconnect();
                return;
            }

            ProcessOnEventReceived(eventcode, content, senderid);
        }

        protected abstract void ProcessOnEventReceived(byte eventcode, object content, int senderid);
    }
}
