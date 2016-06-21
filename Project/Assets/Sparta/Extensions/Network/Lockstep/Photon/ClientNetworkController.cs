using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEngine.Assertions;
using SocialPoint.Utils;
using SocialPoint.Lockstep.Network;
using SocialPoint.Lockstep;
using System.IO;

namespace SocialPoint.Lockstep.Network.Photon
{
    public class ClientNetworkController : IClientNetworkController
    {
        public event Action<string> Log;
        public event Action Connected;
        public event Action Disconnected;
        public event Action<int> OtherConnected;
        public event Action<int> OtherDisconnected;
        public event Action<int, string> Error;
        public event Action<int, string> ServerError;

        public event Action<int, LockstepConfig> LockstepConfigReceived
        {
            add
            {
                _clientLockstepNetwork.LockstepConfigReceived += value;
            }
            remove
            {
                _clientLockstepNetwork.LockstepConfigReceived -= value;
            }
        }

        ClientLockstepNetworkController _clientLockstepNetwork;
        LockstepCommandDataFactory _networkLockstepCommandDataFactory;
        bool _started = false;
        string _version;
        int _playerCount;

        public ClientNetworkController(string version, int playerCount)
        {
            _version = version;
            _playerCount = playerCount;
            RegisterHandlers();
            _clientLockstepNetwork = new ClientLockstepNetworkController(this);
        }

        void OnConnectedToMaster()
        {
            if(PhotonNetwork.room == null)
            {
                PhotonNetwork.JoinRandomRoom();
            }
        }

        void OnPhotonRandomJoinFailed(object[] codeAndMsg)
        {
            PhotonNetwork.CreateRoom(null, new RoomOptions { 
                maxPlayers = (byte)_playerCount,
                plugins = new string[]{ "LockstepServer" }
            }, null);
        }

        void OnDisconnectedFromPhoton()
        {
            if(Disconnected != null)
            {
                Disconnected();
            }
        }

        public void InitLockstep(ClientLockstepController clientLockstep,
                                 LockstepCommandDataFactory commandDataFactory)
        {
            _clientLockstepNetwork.Init(clientLockstep, commandDataFactory);
        }

        void OnEventReceived(byte eventCode, System.Object content, int senderId)
        {
            UnityEngine.Debug.Log("Event received: " + eventCode);
            if(_handlers.ContainsKey(eventCode))
            {
                var handler = _handlers[eventCode];
                handler(new NetworkMessageData() {
                    ConnectionId = senderId,
                    Reader = GetReader(content)
                });
            }
            else if(_syncHandlers.ContainsKey(eventCode))
            {
                var handler = _syncHandlers[eventCode];
                var reader = GetReader(content);
                handler(new SyncNetworkMessageData() {
                    ConnectionId = senderId,
                    Reader = reader,
                    ServerDelay = 0
                });
            }
        }

        public void Start()
        {
            if(!_started)
            {
                PhotonNetwork.ConnectUsingSettings(_version);
                _started = true;
            }
        }

        public void Stop()
        {
            if(_started)
            {
                PhotonNetwork.Disconnect();
            }
        }

        public void SendClientReady()
        {
            _clientLockstepNetwork.SendClientReady();
        }

        void RegisterHandlers()
        {
            CallbackReceiver.Instance.ConnectedToMaster += OnConnectedToMaster;
            CallbackReceiver.Instance.DisconnectedFromPhoton += OnDisconnectedFromPhoton;
            CallbackReceiver.Instance.PhotonPlayerConnected += OnPhotonPlayerConnected;
            CallbackReceiver.Instance.PhotonPlayerDisconnected += OnPhotonPlayerDisconnected;
            CallbackReceiver.Instance.JoinedRoom += OnJoinedRoom;
            CallbackReceiver.Instance.PhotonRandomJoinFailed += OnPhotonRandomJoinFailed;
            PhotonNetwork.OnEventCall += OnEventReceived;
        }

        void OnJoinedRoom()
        {
            if(Connected != null)
            {
                Connected();
            }
        }

        void OnPhotonPlayerDisconnected(PhotonPlayer player)
        {
            if(OtherDisconnected != null)
            {
                OtherDisconnected(player.ID);
            }
        }

        void OnPhotonPlayerConnected(PhotonPlayer player)
        {
            if(OtherConnected != null)
            {
                OtherConnected(player.ID);
            }
        }

        void UnregisterHandlers()
        {
            CallbackReceiver.Instance.ConnectedToMaster -= OnConnectedToMaster;
            CallbackReceiver.Instance.DisconnectedFromPhoton -= OnDisconnectedFromPhoton;
            CallbackReceiver.Instance.PhotonPlayerConnected -= OnPhotonPlayerConnected;
            CallbackReceiver.Instance.PhotonPlayerDisconnected -= OnPhotonPlayerDisconnected;
            CallbackReceiver.Instance.JoinedRoom -= OnJoinedRoom;
            CallbackReceiver.Instance.PhotonRandomJoinFailed -= OnPhotonRandomJoinFailed;
            PhotonNetwork.OnEventCall -= OnEventReceived;
        }

        void WriteLog(string logText)
        {
            if(Log != null)
            {
                Log(logText);
            }
        }

        public void Dispose()
        {
            Stop();
            UnregisterHandlers();
        }

        #region INetworkMessageController implementation

        Dictionary<byte, Action<NetworkMessageData>> _handlers = new Dictionary<byte, Action<NetworkMessageData>>();
        Dictionary<byte, Action<SyncNetworkMessageData>> _syncHandlers = new Dictionary<byte, Action<SyncNetworkMessageData>>();

        public void RegisterHandler(byte msgType, Action<NetworkMessageData> handler)
        {
            _handlers.Add(msgType, handler);
        }

        public void RegisterSyncHandler(byte msgType, Action<SyncNetworkMessageData> handler)
        {
            _syncHandlers.Add(msgType, handler);
        }

        public void UnregisterHandler(byte msgType)
        {
            byte key = msgType;
            if(_handlers.ContainsKey(key))
            {
                _handlers.Remove(key);
            }
            else if(_syncHandlers.ContainsKey(key))
            {
                _syncHandlers.Remove(key);
            }
        }

        byte[] GetMessageBytes(byte msgType, INetworkMessage msg)
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
            IWriterWrapper writerWrapper = new BinaryWriterWrapper(binaryWriter);
            msg.Serialize(writerWrapper);
            return memoryStream.ToArray();
        }

        IReaderWrapper GetReader(object data)
        {
            MemoryStream stream = new MemoryStream((byte[])data);
            BinaryReader binaryReader = new BinaryReader(stream);
            return new BinaryReaderWrapper(binaryReader);
        }

        public void Send(byte msgType, INetworkMessage msg, NetworkReliability channel = NetworkReliability.Reliable, int connectionId = 0)
        {
            var msgBytes = GetMessageBytes(msgType, msg);
            PhotonNetwork.RaiseEvent(msgType, msgBytes, channel == NetworkReliability.Reliable, null);
        }

        public void SendToAll(byte msgType, INetworkMessage msg, NetworkReliability channel = NetworkReliability.Reliable)
        {
            Send(msgType, msg, channel);
        }

        #endregion
    }
}