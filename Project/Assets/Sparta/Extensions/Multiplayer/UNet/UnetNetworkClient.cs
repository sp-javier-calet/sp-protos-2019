using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Utils;
using System;

namespace SocialPoint.Multiplayer
{
    public class UnetNetworkClient : INetworkClient, IDisposable
    {
        List<INetworkClientDelegate> _delegates = new List<INetworkClientDelegate>();
        NetworkClient _client;
        string _serverAddr;
        int _serverPort;

        public const string DefaultServerAddr = "localhost";

        public UnetNetworkClient(string serverAddr=null, int serverPort=UnetNetworkServer.DefaultPort, HostTopology topology=null)
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
        }
            
        void RegisterHandlers()
        {
            UnregisterHandlers();
            _client.RegisterHandler(MsgType.Connect, OnConnectReceived);
            _client.RegisterHandler(MsgType.Disconnect, OnDisconnectReceived);
            _client.RegisterHandler(MsgType.Error, OnErrorReceived);
            for(byte i = MsgType.Highest + 1; i < byte.MaxValue; i++)
            {
                _client.RegisterHandler(i, OnMessageReceived);
            }
        }

        void UnregisterHandlers()
        {
            _client.UnregisterHandler(MsgType.Connect);
            _client.UnregisterHandler(MsgType.Disconnect);
            _client.UnregisterHandler(MsgType.Error);
            for(byte i = MsgType.Highest + 1; i < byte.MaxValue; i++)
            {
                _client.RegisterHandler(i, OnMessageReceived);
            }
        }

        void OnConnectReceived(NetworkMessage umsg)
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnConnected();
            }
        }

        void OnDisconnectReceived(NetworkMessage umsg)
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnDisconnected();
            }
        }

        void OnErrorReceived(NetworkMessage umsg)
        {
            var errMsg = umsg.ReadMessage<ErrorMessage>();
            var err = new Error(errMsg.errorCode, errMsg.ToString());
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnError(err);
            }
        }

        void OnMessageReceived(NetworkMessage umsg)
        {
            byte type = UnetNetworkMessage.ConvertType(umsg.msgType);
            var msg = new ReceivedNetworkMessage(type, umsg.channelId, new UnetNetworkReader(umsg.reader));
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnMessageReceived(msg);
            }
        }

        public void Connect()
        {
            if(_client.isConnected)
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

        public INetworkMessage CreateMessage(NetworkMessageInfo info)
        {
            return new UnetNetworkMessage(info, new NetworkConnection[]{_client.connection});
        }

        public void AddDelegate(INetworkClientDelegate dlg)
        {
            _delegates.Add(dlg);
        }

        public void RemoveDelegate(INetworkClientDelegate dlg)
        {
            _delegates.Remove(dlg);
        }

    }
}