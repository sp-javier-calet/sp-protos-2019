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
    public class PhotonNetworkConfig
    {
        public string GameVersion;
        public string RoomName;
        public RoomOptions RoomOptions;
        public byte[] UnreliableChannels;
    }

    public abstract class PhotonNetworkBase : Photon.MonoBehaviour, IDisposable
	{
        PhotonNetworkConfig _config;

        const int ConnectionError = 1;
        const int CreateRoomError = 2;
        const int CustomAuthError = 3;

        public void Init(PhotonNetworkConfig config)
        {
            _config = config;
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
            if(string.IsNullOrEmpty(_config.RoomName))
            {
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                PhotonNetwork.JoinRoom(_config.RoomName);
            }
        }

        void OnPhotonRandomJoinFailed()
        {
            PhotonNetwork.CreateRoom(_config.RoomName, _config.RoomOptions, null);
        }

        void OnFailedToConnectToPhoton(DisconnectCause cause)
        {
            var err = new Error(ConnectionError, "Failed to connect: " + cause);
            OnNetworkError(err);
        }

        void OnPhotonCreateRoomFailed(object[] codeAndMsg)
        {
            var err = new Error(CreateRoomError, "Failed to create room: " +  StringUtils.Join(codeAndMsg, " "));
            OnNetworkError(err);
        }

        void OnJoinedRoom()
        {
            PhotonNetwork.OnEventCall += OnEventReceived;
            OnConnected();
        }

        void OnLeftRoom()
        {
            PhotonNetwork.Disconnect();
        }

        void OnDisconnectedFromPhoton()
        {
            PhotonNetwork.OnEventCall -= OnEventReceived;
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

        protected static byte GetPlayerClientId(PhotonPlayer player)
        {
            return (byte)player.ID;
        }

        protected static PhotonPlayer GetClientIdPlayer(byte clientId)
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

        bool IsChannelReliable(byte channelId)
        {
            if(_config.UnreliableChannels == null)
            {
                return true;
            }
            for(var i = 0; i < _config.UnreliableChannels.Length; i++)
            {
                if(_config.UnreliableChannels[i] == channelId)
                {
                    return false;
                }
            }
            return true;
        }

        public void SendNetworkMessage(NetworkMessageData info, byte[] data)
        {
            var options = new RaiseEventOptions();
            options.SequenceChannel = info.ChannelId;

            var serverId = PhotonNetwork.room.masterClientId;
            if(PhotonNetwork.player.ID != serverId)
            {
                // clients always send to server
                options.TargetActors = new int[]{ serverId };
            }
            else if(info.ClientId != 0)
            {
                var player = GetClientIdPlayer(info.ClientId);
                if(player == null)
                {
                    throw new InvalidOperationException("Could not find player with client id "+info.ClientId+".");
                }
                options.TargetActors = new int[]{ player.ID };
            }
            var reliable = IsChannelReliable(info.ChannelId);
            var content = new object[]{ info.ChannelId, data };
            PhotonNetwork.RaiseEvent(info.MessageType, content, reliable, options);
        }

        void OnEventReceived(byte eventcode, object content, int senderid)
        {
            var contentArr = (object[])content;
            byte clientId = 0;
            if(senderid != PhotonNetwork.room.masterClientId)
            {
                clientId = GetPlayerClientId(GetClientIdPlayer((byte)senderid));
            }
            var channelId = (byte)contentArr[0];
            var data = (byte[])contentArr[1];
            var info = new NetworkMessageData {
                MessageType = eventcode,
                ClientId = clientId,
                ChannelId = channelId
            };
            var reader = new SystemBinaryReader(new MemoryStream(data));
            OnMessageReceived(info, reader);
        }
    }
}
