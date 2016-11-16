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
        public const bool DefaultCleanupCache = true;
        public const bool DefaultPublishUserId = false;

        public bool IsVisible = DefaultIsVisible;
        public bool IsOpen = DefaultIsOpen;
        public byte MaxPlayers = DefaultMaxPlayers;
        public int PlayerTtl = DefaultPlayerTtl;
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
                CleanupCacheOnLeave = CleanupCache,
                CustomRoomPropertiesForLobby = CustomLobbyProperties,
                Plugins = Plugins,
                PublishUserId = PublishUserId
            };
        }
    }

    [Serializable]
    public class CustomInternalNetworkConfig
    {
        const int DefaultUpdateInterval = 100000;
        const int DefaultUpdateIntervalOnSerialize = 100000;
        const int DefaultMaximumTransferUnit = 1500;
        const int DefaultSentCountAllowance = 10; // Allow for big lags
        const int DefaultQuickResendAttempts = 0; // SpeedUp from second repeat on. This avoid resending repeats too fast

        public int UpdateInterval = DefaultUpdateInterval;
        public int UpdateIntervalOnSerialize = DefaultUpdateIntervalOnSerialize;
        public int MaximumTransferUnit = DefaultMaximumTransferUnit;
        public int SentCountAllowance = DefaultSentCountAllowance;
        public int QuickResendAttempts = DefaultQuickResendAttempts;
    }

    [Serializable]
    public class PhotonNetworkConfig
    {
        public string GameVersion;
        public string RoomName;
        public bool EnableCustomInteralPhotonNetworkConfig = true;
        public CustomInternalNetworkConfig CustomInternalNetworkConfig = new CustomInternalNetworkConfig();
        public PhotonNetworkRoomConfig RoomOptions = new PhotonNetworkRoomConfig();
    }

    public abstract class PhotonNetworkBase : Photon.MonoBehaviour, IDisposable
    {
        public PhotonNetworkConfig Config;

        const int ConnectionError = 1;
        const int CreateRoomError = 2;
        const int CustomAuthError = 3;

        CustomInternalNetworkConfig _originalInternalNetworkConfig = new CustomInternalNetworkConfig();
        bool _pendingOutgoingCommands = false;

        [Obsolete("Use the Config property")]
        public void Init(PhotonNetworkConfig config)
        {
            Config = config;

            SaveOriginalInternalPhotonSettings();
            SetCustomInternalPhotonSettings();
        }

        void SaveOriginalInternalPhotonSettings()
        {
            _originalInternalNetworkConfig.UpdateInterval = PhotonNetwork.photonMono.updateInterval;
            _originalInternalNetworkConfig.UpdateIntervalOnSerialize = PhotonNetwork.photonMono.updateIntervalOnSerialize;

            _originalInternalNetworkConfig.SentCountAllowance = PhotonNetwork.networkingPeer.SentCountAllowance;
            _originalInternalNetworkConfig.QuickResendAttempts = PhotonNetwork.networkingPeer.QuickResendAttempts;

            _originalInternalNetworkConfig.MaximumTransferUnit = PhotonNetwork.networkingPeer.MaximumTransferUnit;
        }

        void SetCustomInternalPhotonSettings()
        {
            if(Config.EnableCustomInteralPhotonNetworkConfig)
            {
                if(PhotonNetwork.connected)
                {
                    PhotonNetwork.photonMono.updateInterval = Config.CustomInternalNetworkConfig.UpdateInterval;
                    PhotonNetwork.photonMono.updateIntervalOnSerialize = Config.CustomInternalNetworkConfig.UpdateIntervalOnSerialize;

                    PhotonNetwork.networkingPeer.SentCountAllowance = Config.CustomInternalNetworkConfig.SentCountAllowance;
                    PhotonNetwork.networkingPeer.QuickResendAttempts = (byte) Config.CustomInternalNetworkConfig.QuickResendAttempts;
                }
                else
                {
                    PhotonNetwork.networkingPeer.MaximumTransferUnit = Config.CustomInternalNetworkConfig.MaximumTransferUnit;
                }
            }
        }

        void RestoreInternalCustomPhotonSettings()
        {
            if(Config.EnableCustomInteralPhotonNetworkConfig)
            {
                PhotonNetwork.photonMono.updateInterval = _originalInternalNetworkConfig.UpdateInterval;
                PhotonNetwork.photonMono.updateIntervalOnSerialize = _originalInternalNetworkConfig.UpdateIntervalOnSerialize;

                PhotonNetwork.networkingPeer.SentCountAllowance = _originalInternalNetworkConfig.SentCountAllowance;
                PhotonNetwork.networkingPeer.QuickResendAttempts = (byte) _originalInternalNetworkConfig.QuickResendAttempts;

                PhotonNetwork.networkingPeer.MaximumTransferUnit = _originalInternalNetworkConfig.MaximumTransferUnit;
            }
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
            if(_pendingOutgoingCommands)
            {
                _pendingOutgoingCommands = false;
                PhotonNetwork.SendOutgoingCommands();
            }
        }

        protected void DoConnect()
        {
            DoDisconnect();
            PhotonNetwork.ConnectUsingSettings(Config.GameVersion);
        }

        protected void DoDisconnect()
        {
            if(PhotonNetwork.connected)
            {
                PhotonNetwork.Disconnect();
            }
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
            if(string.IsNullOrEmpty(Config.RoomName))
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
            if(!PhotonNetwork.autoJoinLobby)
            {
                PhotonNetwork.JoinLobby();
            }
        }

        void OnJoinedLobby()
        {
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
            JoinOrCreateRoom();
        }

        public void OnPhotonJoinRoomFailed(object[] codeAndMsg)
        {
            var err = new Error(ConnectionError, "Failed to join: " + StringUtils.Join(codeAndMsg, " "));
            OnNetworkError(err);
        }

        public void OnFailedToConnectToPhoton(DisconnectCause cause)
        {
            var err = new Error(ConnectionError, "Failed to connect: " + cause);
            OnNetworkError(err);
        }

        public void OnPhotonCreateRoomFailed(object[] codeAndMsg)
        {
            var err = new Error(CreateRoomError, "Failed to create room: " + StringUtils.Join(codeAndMsg, " "));
            OnNetworkError(err);
        }

        void OnJoinedRoom()
        {
            PhotonNetwork.OnEventCall += OnEventReceived;
            SetCustomInternalPhotonSettings();
            OnConnected();
        }

        void OnLeftRoom()
        {
            PhotonNetwork.Disconnect();
        }

        void OnDisconnectedFromPhoton()
        {
            PhotonNetwork.OnEventCall -= OnEventReceived;
            RestoreInternalCustomPhotonSettings();
            OnDisconnected();
        }

        void OnConnectionFail(DisconnectCause cause)
        {
            var err = new Error(ConnectionError, "Failed to connect: " + cause);
            OnNetworkError(err);
            OnDisconnected();
        }

        void OnCustomAuthenticationFailed(string debugMessage)
        {
            var err = new Error(CustomAuthError, "Custom Authentication failed: " + debugMessage);
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
            _pendingOutgoingCommands = true;
        }

        void OnEventReceived(byte eventcode, object content, int senderid)
        {
            if(eventcode == EventCode.ErrorInfo)
            {
                var err = new Error((string)content);
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
