using System;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.IO;
using SocialPoint.Utils;
using UnityEngine;

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
        public Dictionary<object, object> CustomProperties = new Dictionary<object, object>();
        public string[] CustomLobbyProperties;
        public string[] Plugins;
        public string[] ForcedPlugins;

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
                Plugins = ForcedPlugins ?? Plugins,
                PublishUserId = PublishUserId
            };
        }
    }

    [Serializable]
    public class PhotonNetworkConfig
    {
        public const bool DefaultCreateRoom = true;
        public const ExitGames.Client.Photon.ConnectionProtocol DefaultProtocol = ExitGames.Client.Photon.ConnectionProtocol.Udp;

        public string GameVersion;
        public string RoomName;
        public bool CreateRoom;
        public CustomPhotonConfig CustomPhotonConfig;
        public PhotonNetworkRoomConfig RoomOptions;
        public CloudRegionCode ForceRegion;
        public string ForceAppId;
        public string ForceServer;
        public ExitGames.Client.Photon.ConnectionProtocol Protocol = DefaultProtocol;
        public PhotonNetworkTroubleshootingConfig TroubleshootingConfig = new PhotonNetworkTroubleshootingConfig();

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
            if(c.RoomOptions.ForcedPlugins != null)
            {
                rfg.ForcedPlugins = new string[c.RoomOptions.ForcedPlugins.Length];
                Array.Copy(c.RoomOptions.ForcedPlugins, rfg.ForcedPlugins, c.RoomOptions.ForcedPlugins.Length);
            }
            rfg.PublishUserId = c.RoomOptions.PublishUserId;
            RoomOptions = rfg;
        }
    }

    [Serializable]
    public class PhotonNetworkTroubleshootingConfig
    {
        public int MaxReconnectAttempts = 6;
        public float ReconnectInterval = 3.0f;
        public bool ReconnectMidGameEnabled = true;
    }

    public interface IPhotonNetworkDelegate
    {
        void OnCreatedRoom();

        void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable properties);

        void OnAutoReconnected();

        void OnConnectingError(DisconnectCause cause);

        void OnTryReconnect(int numReconnects, DisconnectCause cause);
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

        enum ReconnectionOrigin
        {
            FromFailedConnectAttempt,
            FromFailedRunningConnection,
        }


        bool _reconnecting;
        int _reconnectAttempts;
        ReconnectionOrigin _reconnectionOrigin;
        Coroutine _reconnectionCoroutine;

        protected List<IPhotonNetworkDelegate> _photonDelegates = new List<IPhotonNetworkDelegate>();

        public enum ErrorType
        {
            ConnectionError,
            ConnectionLostError,
            CreateRoomError,
            JoinRoomError,
            CustomAuthError,
        }

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

        public void AddPhotonDelegate(IPhotonNetworkDelegate dlg)
        {
            if(!_photonDelegates.Contains(dlg))
            {
                _photonDelegates.Add(dlg);
            }
        }

        public void RemovePhotonDelegate(IPhotonNetworkDelegate dlg)
        {
            _photonDelegates.Remove(dlg);
        }

        protected virtual void DoConnect()
        {
            RegisterAsMessageTarget();

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
                if(!string.IsNullOrEmpty(Config.ForceAppId))
                {
                    PhotonNetwork.PhotonServerSettings.AppID = Config.ForceAppId;
                }
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

        IEnumerator ReconnectCoroutine(DisconnectCause cause)
        {
            while(_reconnectAttempts < Config.TroubleshootingConfig.MaxReconnectAttempts)
            {
                yield return new WaitForSecondsRealtime(Config.TroubleshootingConfig.ReconnectInterval);
                _reconnectAttempts++;
                OnTryReconnect(_reconnectAttempts, cause);
                DoConnect();
            }
        }

        void StartReconnection(ReconnectionOrigin origin, DisconnectCause cause)
        {
            StopReconnection();
            _reconnecting = true;
            _reconnectAttempts = 0;
            _reconnectionOrigin = origin;
            _reconnectionCoroutine = StartCoroutine(ReconnectCoroutine(cause));
        }

        void StopReconnection()
        {
            if(_reconnectionCoroutine != null)
            {
                StopCoroutine(_reconnectionCoroutine);
            }
            _reconnectionCoroutine = null;
            _reconnectAttempts = 0;
            _reconnecting = false;
        }


        public void Dispose()
        {
            UnregisterAsMessageTarget();
            StopReconnection();
            DoDisconnect();
            Destroy(this);
        }

        void RegisterAsMessageTarget()
        {
            if(PhotonNetwork.SendMonoMessageTargets == null)
            {
                PhotonNetwork.SendMonoMessageTargets = new HashSet<GameObject>();
            }
            PhotonNetwork.SendMonoMessageTargets.Add(this.gameObject);
        }

        void UnregisterAsMessageTarget()
        {
            if(PhotonNetwork.SendMonoMessageTargets != null)
            {
                PhotonNetwork.SendMonoMessageTargets.Remove(this.gameObject);
            }
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

        abstract protected bool IsRecoverableDisconnectCause(DisconnectCause cause);

        abstract internal void OnNetworkError(Error err);

        abstract protected void OnMessageReceived(NetworkMessageData data, IReader reader);

        abstract protected void OnConnected();

        void OnReconnected()
        {
            var delegates = new List<IPhotonNetworkDelegate>(_photonDelegates);
            for(var i = 0; i < delegates.Count; i++)
            {
                delegates[i].OnAutoReconnected();
            }
        }

        void OnConnectingError(DisconnectCause cause)
        {
            var delegates = new List<IPhotonNetworkDelegate>(_photonDelegates);
            for(var i = 0; i < delegates.Count; i++)
            {
                delegates[i].OnConnectingError(cause);
            }
        }

        void OnTryReconnect(int numReconnects, DisconnectCause cause)
        {
            var delegates = new List<IPhotonNetworkDelegate>(_photonDelegates);
            for(var i = 0; i < delegates.Count; i++)
            {
                delegates[i].OnTryReconnect(numReconnects, cause);
            }
        }

        virtual protected void OnDisconnected()
        {
            _state = ConnState.Disconnected;
            Config.CustomPhotonConfig.RestorePhotonConfig();
        }

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
            var err = new Error((int)ErrorType.JoinRoomError, "Failed to join: " + StringUtils.Join(codeAndMsg, " "));
            OnNetworkError(err);
            StopReconnection();
        }

        public void OnFailedToConnectToPhoton(DisconnectCause cause)
        {
            Log.e("Failed to connect: " + cause);
            if(State != ConnState.Connecting)
            {
                return;
            }

            if(IsRecoverableDisconnectCause(cause) && _reconnectAttempts < Config.TroubleshootingConfig.MaxReconnectAttempts)
            {
                if(!_reconnecting)
                {
                    StartReconnection(ReconnectionOrigin.FromFailedConnectAttempt, cause);
                }
            }
            else
            {
                int errorCode = (_reconnecting && _reconnectionOrigin == ReconnectionOrigin.FromFailedRunningConnection) ? (int)ErrorType.ConnectionLostError : (int)ErrorType.ConnectionError;
                var err = new Error(errorCode, "Failed to connect: " + cause);
                OnConnectingError(cause);
                OnNetworkError(err);
                StopReconnection();
            }
        }

        public void OnPhotonCreateRoomFailed(object[] codeAndMsg)
        {
            Log.e("Failed to join: " + StringUtils.Join(codeAndMsg, " "));
            if(State != ConnState.Connecting)
            {
                return;
            }
            var err = new Error((int)ErrorType.CreateRoomError, "Failed to create room: " + StringUtils.Join(codeAndMsg, " "));
            OnNetworkError(err);
            DoDisconnect();
            OnDisconnected();
            StopReconnection();
        }

        protected void OnCreatedRoom()
        {
            var delegates = new List<IPhotonNetworkDelegate>(_photonDelegates);
            for(var i = 0; i < delegates.Count; i++)
            {
                delegates[i].OnCreatedRoom();
            }
        }

        protected void OnJoinedRoom()
        {
            if(State != ConnState.Connecting)
            {
                return;
            }
            _state = ConnState.Connected;
            Log.d("[PhotonNetworkBase] Registering to OnEventCall");
            PhotonNetwork.OnEventCall += OnEventReceived;
            Config.CustomPhotonConfig.SetConfigOnJoinedRoom();
            if(!_reconnecting || _reconnectionOrigin == ReconnectionOrigin.FromFailedConnectAttempt)
            {
                OnConnected();
            }
            else
            {
                OnReconnected();
            }

            StopReconnection();
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
            PhotonNetwork.OnEventCall -= OnEventReceived;
            if(State == ConnState.Disconnected)
            {
                return;
            }
            if(!_reconnecting)
            {
                OnDisconnected();
            }
        }

        void OnConnectionFail(DisconnectCause cause)
        {
            Log.e("Failed to connect: " + cause);
            if(State != ConnState.Connected)
            {
                return;
            }
            if(Config.TroubleshootingConfig.ReconnectMidGameEnabled && IsRecoverableDisconnectCause(cause))
            {
                if(!_reconnecting)
                {
                    StartReconnection(ReconnectionOrigin.FromFailedRunningConnection, cause);
                }
            }
            else
            {
                var err = new Error((int)ErrorType.ConnectionLostError, "Failed to connect: " + cause);
                OnConnectingError(cause);
                OnNetworkError(err);
                OnDisconnected();
                StopReconnection();
            }
        }

        void OnCustomAuthenticationFailed(string debugMessage)
        {
            if(State != ConnState.Connected)
            {
                return;
            }
            var err = new Error((int)ErrorType.CustomAuthError, "Custom Authentication failed: " + debugMessage);
            OnNetworkError(err);
            OnDisconnected();
            StopReconnection();
        }

        void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
        {
            var delegates = new List<IPhotonNetworkDelegate>(_photonDelegates);
            for(var i = 0; i < delegates.Count; i++)
            {
                delegates[i].OnPhotonCustomRoomPropertiesChanged(propertiesThatChanged);
            }
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
                StopReconnection();
                return;
            }

            ProcessOnEventReceived(eventcode, content, senderid);
        }

        protected abstract void ProcessOnEventReceived(byte eventcode, object content, int senderid);
    }
}
