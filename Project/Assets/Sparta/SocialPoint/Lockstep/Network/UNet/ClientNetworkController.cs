using System;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEngine.Assertions;
using SocialPoint.Utils;
using SocialPoint.Lockstep.Network;
using SocialPoint.Lockstep;
using System.IO;
using SocialPoint.Lockstep.Network.UNet;

namespace SocialPoint.Lockstep.Network.UNet
{
    public class ClientNetworkController : IDisposable, INetworkMessageController
    {
        const int _packetSize = 1440;

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

        NetworkClient _client;

        public NetworkClient NetworkClient
        {
            get
            {
                return _client;
            }
        }

        string _serverAddress;
        bool _initialized;
        int _port;
        ClientLockstepNetworkController _clientLockstepNetwork;
        LockstepCommandDataFactory _networkLockstepCommandDataFactory;
        bool _started;

        public short PlayerConnectedMsgType { get; protected set; }

        public short PlayerDisconnectedMsgType { get; protected set; }

        public ClientNetworkController(string serverAddress,
                                       int port,
                                       short playerConnectedMsgType = 1000, 
                                       short playerDisconnectedMsgType = 1001)
        {
            _serverAddress = serverAddress;
            _port = port;
            InitClient();
            PlayerConnectedMsgType = playerConnectedMsgType;
            PlayerDisconnectedMsgType = playerDisconnectedMsgType;
            RegisterHandlers();
            _clientLockstepNetwork = new ClientLockstepNetworkController(this);
        }

        void InitClient()
        {
            NetworkTransport.Init();
            var config = new ConnectionConfig{ PacketSize = _packetSize };
            config.AddChannel(QosType.Reliable);
            config.AddChannel(QosType.Unreliable);
            var topology = new HostTopology(config, 1);
            _client = new NetworkClient();
            _client.Configure(topology);
        }

        public void InitLockstep(ClientLockstepController clientLockstep,
                                 LockstepCommandDataFactory commandDataFactory)
        {
            _clientLockstepNetwork.Init(clientLockstep, commandDataFactory);
        }

        public void Start()
        {
            if(!_started)
            {
                _started = true;
                _client.Connect(_serverAddress, _port);
                return;
            }
        }

        public void Stop()
        {
            if(_started)
            {
                _client.Disconnect();
                _started = false;
            }
        }

        public void SendClientReady()
        {
            _clientLockstepNetwork.SendClientReady();
        }

        void RegisterHandlers()
        {
            _client.RegisterHandler(MsgType.Connect, OnConnectReceived);
            _client.RegisterHandler(MsgType.Disconnect, OnDisconnectReceived);
            _client.RegisterHandler(MsgType.Error, OnErrorReceived);
            _client.RegisterHandler(PlayerConnectedMsgType, OnPlayerConnectedReceived);
            _client.RegisterHandler(PlayerDisconnectedMsgType, OnPlayerDisconnectedReceived);
        }

        void UnregisterHandlers()
        {
            _client.UnregisterHandler(MsgType.Connect);
            _client.UnregisterHandler(MsgType.Disconnect);
            _client.UnregisterHandler(MsgType.Error);
            _client.UnregisterHandler(PlayerConnectedMsgType);
            _client.UnregisterHandler(PlayerDisconnectedMsgType);
        }

        void OnConnectReceived(NetworkMessage netMsg)
        {
            if(Connected != null)
            {
                Connected();
            }
        }

        void OnDisconnectReceived(NetworkMessage netMsg)
        {
            if(Disconnected != null)
            {
                Disconnected();
            }
        }

        void OnErrorReceived(NetworkMessage netMsg)
        {
            var error = netMsg.ReadMessage<ErrorMessage>();

            if(Error != null)
            {
                Error(error.errorCode, error.ToString());
            }

            WriteLog(string.Format("Network error {0}: {1}", error.errorCode, error.ToString()));
        }

        void OnPlayerConnectedReceived(NetworkMessage netMsg)
        {
            if(OtherConnected != null)
            {
                OtherConnected(netMsg.ReadMessage<IntegerMessage>().value);
            }
        }

        void OnPlayerDisconnectedReceived(NetworkMessage netMsg)
        {
            if(OtherDisconnected != null)
            {
                OtherDisconnected(netMsg.ReadMessage<IntegerMessage>().value);
            }
        }

        void OnServerError(NetworkMessage msg)
        {
            var error = msg.ReadMessage<ErrorMessage>();

            if(ServerError != null)
            {
                ServerError(error.errorCode, error.ToString());
            }

            WriteLog(string.Format("Server error {0}: {1}", error.errorCode, error.ToString()));
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
            if(_clientLockstepNetwork != null)
            {
                _clientLockstepNetwork.Dispose();
            }
            _client.Shutdown();
        }

        #region INetworkMessageController implementation

        Dictionary<short, BaseNetworkMessageHandler> _handlers = new Dictionary<short, BaseNetworkMessageHandler>();

        public void RegisterHandler(short msgType, Action<NetworkMessageData> handler)
        {
            var msgHandler = new NetworkMessageHandler(msgType, handler);
            _handlers.Add(msgType, msgHandler);
            msgHandler.Register(_client);
        }

        public void RegisterSyncHandler(short msgType, Action<SyncNetworkMessageData> handler)
        {
            var msgHandler = new SyncNetworkMessageHandler(msgType, handler);
            _handlers.Add(msgType, msgHandler);
            msgHandler.Register(_client);
        }

        public void UnregisterHandler(short msgType)
        {
            BaseNetworkMessageHandler handler;
            if(_handlers.TryGetValue(msgType, out handler))
            {
                handler.Unregister(_client);
                _handlers.Remove(msgType);
            }
        }

        int GetChannelIdByNetworkReliability(NetworkReliability reliability)
        {
            return reliability == NetworkReliability.Reliable ? 0 : 1;
        }

        public void Send(short msgType, INetworkMessage msg, NetworkReliability reliability = NetworkReliability.Reliable, int connectionId = 0)
        {
            NetworkMessageWrapper message = new NetworkMessageWrapper(msg);
            var channelId = GetChannelIdByNetworkReliability(reliability);
            _client.SendByChannel(msgType, message, channelId);
        }

        public void SendToAll(short msgType, INetworkMessage msg, NetworkReliability reliability = NetworkReliability.Reliable)
        {
            Send(msgType, msg, reliability);
        }

        #endregion
    }
}