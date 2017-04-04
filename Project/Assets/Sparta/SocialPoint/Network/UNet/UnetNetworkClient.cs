using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Utils;
using System;

namespace SocialPoint.Network
{
    public sealed class UnetNetworkClient : INetworkClient, IDisposable
    {
        INetworkMessageReceiver _receiver;
        List<INetworkClientDelegate> _delegates = new List<INetworkClientDelegate>();
        NetworkClient _client;
        string _serverAddr;
        int _serverPort;

        public const string DefaultServerAddr = "localhost";

        public byte ClientId
        {
            get
            {
                if(!Connected)
                {
                    return 0;
                }
                return (byte)_client.connection.connectionId;
            }
        }

        public bool Connected
        {
            get
            {
                return _client != null && _client.isConnected;
            }
        }

        public UnetNetworkClient(string serverAddr = null, int serverPort = UnetNetworkServer.DefaultPort, HostTopology topology = null)
        {
            if(string.IsNullOrEmpty(serverAddr))
            {
                serverAddr = DefaultServerAddr;
            }
            _serverAddr = serverAddr;
            _serverPort = serverPort;
            NetworkTransport.Init();
            _client = new NetworkClient();
            if(topology != null)
            {
                _client.Configure(topology);
            }
            RegisterHandlers();
        }

        public void Dispose()
        {
            Disconnect();
            UnregisterHandlers();
            _client.Shutdown();
            _client = null;
            _delegates.Clear();
            _delegates = null;
            _receiver = null;
        }

        void RegisterHandlers()
        {
            UnregisterHandlers();
            _client.RegisterHandler(UnityEngine.Networking.MsgType.Connect, OnConnectReceived);
            _client.RegisterHandler(UnityEngine.Networking.MsgType.Disconnect, OnDisconnectReceived);
            _client.RegisterHandler(UnityEngine.Networking.MsgType.Error, OnErrorReceived);
            _client.RegisterHandler(UnetMsgType.Fail, OnFailReceived);
            for(byte i = UnetMsgType.Highest + 1; i < byte.MaxValue; i++)
            {
                _client.RegisterHandler(i, OnMessageReceived);
            }
        }

        void UnregisterHandlers()
        {
            _client.UnregisterHandler(UnityEngine.Networking.MsgType.Connect);
            _client.UnregisterHandler(UnityEngine.Networking.MsgType.Disconnect);
            _client.UnregisterHandler(UnityEngine.Networking.MsgType.Error);
            _client.UnregisterHandler(UnetMsgType.Fail);
            for(byte i = UnetMsgType.Highest + 1; i < byte.MaxValue; i++)
            {
                _client.UnregisterHandler(i);
            }
        }

        void OnConnectReceived(NetworkMessage umsg)
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientConnected();
            }
        }

        void OnDisconnectReceived(NetworkMessage umsg)
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientDisconnected();
            }
        }

        void OnErrorReceived(NetworkMessage umsg)
        {
            var errMsg = umsg.ReadMessage<ErrorMessage>();
            var err = new Error(errMsg.errorCode, errMsg.ToString());
            OnNetworkError(err);
        }

        void OnFailReceived(NetworkMessage umsg)
        {
            var err = Error.FromString(umsg.reader.ReadString());
            OnNetworkError(err);
            Disconnect();
        }

        void OnNetworkError(Error err)
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnNetworkError(err);
            }
        }

        void OnMessageReceived(NetworkMessage umsg)
        {
            var data = new NetworkMessageData {
                MessageType = UnetMsgType.ConvertType(umsg.msgType),
            };                
            if(_receiver != null)
            {
                _receiver.OnMessageReceived(data, new UnetNetworkReader(umsg.reader));
            }
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnMessageReceived(data);
            }
        }

        public void Connect()
        {
            if(Connected)
            {
                return;
            }
            _client.Connect(_serverAddr, _serverPort);
        }

        public void Disconnect()
        {
            if(!_client.isConnected)
            {
                return;
            }
            if(_client.connection != null && _client.connection.hostId >= 0)
            {
                NetworkTransport.RemoveHost(_client.connection.hostId);
            }
            _client.Disconnect();
        }

        public INetworkMessage CreateMessage(NetworkMessageData info)
        {
            return new UnetNetworkMessage(info, new NetworkConnection[]{ _client.connection });
        }

        public void AddDelegate(INetworkClientDelegate dlg)
        {
            _delegates.Add(dlg);
            if(Connected && dlg != null)
            {
                dlg.OnClientConnected();
            }
        }

        public void RemoveDelegate(INetworkClientDelegate dlg)
        {
            _delegates.Remove(dlg);
        }

        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        public int GetDelay(int serverTimestamp)
        {
            byte error;
            return NetworkTransport.GetRemoteDelayTimeMS(_client.connection.hostId, _client.connection.connectionId, serverTimestamp, out error);
        }

        public bool LatencySupported
        {
            get
            {
                return false;
            }
        }

        public int Latency
        {
            get
            {
                DebugUtils.Assert(LatencySupported);
                return -1;
            }
        }
    }
}