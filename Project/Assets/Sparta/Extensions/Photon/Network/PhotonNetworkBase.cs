using Photon;
using SocialPoint.Base;
using SocialPoint.Utils;
using SocialPoint.IO;
using System;
using System.IO;
using System.Collections.Generic;

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
        const int DefaultUpdateInterval = 10000; // Update Interval
        const int DefaultUpdateIntervalOnSerialize = 10000; // Update Interval OnSerialize
        const int DefaultMaximumTransferUnit = 2000; // How much data we can transfer
        const int DefaultSentCountAllowance = 10; // Allow for big lags
        const int DefaultQuickResendAttempts = 1; // SpeedUp from second repeat on. This avoid resending repeats too fast
        
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
        public CustomInternalNetworkConfig CustomInternalNetworkConfig;
        public PhotonNetworkRoomConfig RoomOptions = new PhotonNetworkRoomConfig();
    }

    public abstract class PhotonNetworkBase : Photon.MonoBehaviour, IDisposable
    {
        PhotonNetworkConfig _config;

        const int ConnectionError = 1;
        const int CreateRoomError = 2;
        const int CustomAuthError = 3;

        public CustomInternalNetworkConfig _originalInternalNetworkConfig;

        public void Init(PhotonNetworkConfig config)
        {
            _config = config;
        }

        void SaveOriginalInternalPhotonSettings()
        {
            _originalInternalNetworkConfig.UpdateInterval = PhotonNetwork.photonMono.updateInterval;
            _originalInternalNetworkConfig.UpdateIntervalOnSerialize = PhotonNetwork.photonMono.updateIntervalOnSerialize;
            _originalInternalNetworkConfig.MaximumTransferUnit = PhotonNetwork.networkingPeer.MaximumTransferUnit;
            _originalInternalNetworkConfig.SentCountAllowance = PhotonNetwork.networkingPeer.SentCountAllowance;
            _originalInternalNetworkConfig.QuickResendAttempts = PhotonNetwork.networkingPeer.QuickResendAttempts;
        }

        void SetCustomInternalPhotonSettings()
        {
            if(_config.EnableCustomInteralPhotonNetworkConfig)
            {
                PhotonNetwork.photonMono.updateInterval = _config.CustomInternalNetworkConfig.UpdateInterval;
                PhotonNetwork.photonMono.updateIntervalOnSerialize = _config.CustomInternalNetworkConfig.UpdateIntervalOnSerialize;
                PhotonNetwork.networkingPeer.MaximumTransferUnit = _config.CustomInternalNetworkConfig.MaximumTransferUnit;
                PhotonNetwork.networkingPeer.SentCountAllowance = _config.CustomInternalNetworkConfig.SentCountAllowance;
                PhotonNetwork.networkingPeer.QuickResendAttempts = (byte) _config.CustomInternalNetworkConfig.QuickResendAttempts;
            }
        }

        void RestoreInternalCustomPhotonSettings()
        {
            if(_config.EnableCustomInteralPhotonNetworkConfig)
            {
                PhotonNetwork.photonMono.updateInterval = _originalInternalNetworkConfig.UpdateInterval;
                PhotonNetwork.photonMono.updateIntervalOnSerialize = _originalInternalNetworkConfig.UpdateIntervalOnSerialize;
                PhotonNetwork.networkingPeer.MaximumTransferUnit = _originalInternalNetworkConfig.MaximumTransferUnit;
                PhotonNetwork.networkingPeer.SentCountAllowance = _originalInternalNetworkConfig.SentCountAllowance;
                PhotonNetwork.networkingPeer.QuickResendAttempts = (byte) _originalInternalNetworkConfig.QuickResendAttempts;
            }
        }

        void Awake()
        {
            if(_config == null)
            {
                _config = new PhotonNetworkConfig();
            }
        }

        protected void DoConnect()
        {
            PhotonNetwork.ConnectUsingSettings(_config.GameVersion);
        }

        protected void DoDisconnect()
        {
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
                return _config.RoomOptions == null ? null : _config.RoomOptions.ToPhoton();
            }
        }

        void JoinOrCreateRoom()
        {
            if(string.IsNullOrEmpty(_config.RoomName))
            {
                PhotonNetwork.CreateRoom(_config.RoomName, PhotonRoomOptions, null);
            }
            else
            {
                PhotonNetwork.JoinOrCreateRoom(_config.RoomName, PhotonRoomOptions, null);
            }
        }

        abstract protected void OnNetworkError(Error err);

        abstract protected void OnConnected();

        abstract protected void OnDisconnected();

        abstract protected void OnMessageReceived(NetworkMessageData data, IReader reader);

        #region photon callbacks

        void OnConnectedToMaster()
        {
            SaveOriginalInternalPhotonSettings();

            if(!PhotonNetwork.autoJoinLobby)
            {
                PhotonNetwork.JoinLobby();
            }
        }

        void OnJoinedLobby()
        {
            if(string.IsNullOrEmpty(_config.RoomName))
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
        }

        void OnCustomAuthenticationFailed(string debugMessage)
        {
            var err = new Error(CustomAuthError, "Custom Authentication failed: " + debugMessage);
            OnNetworkError(err);
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
            PhotonNetwork.SendOutgoingCommands();
        }

        void OnEventReceived(byte eventcode, object content, int senderid)
        {
            if(eventcode == EventCode.ErrorInfo)
            {
                var err = new Error((string)content);
                OnNetworkError(err);
                PhotonNetwork.Disconnect();
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
