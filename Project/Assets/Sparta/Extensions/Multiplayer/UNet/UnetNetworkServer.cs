using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Collections.Generic;
using System;
using SocialPoint.Base;

namespace SocialPoint.Multiplayer
{
    public class UnetNetworkServer : INetworkServer, IDisposable
    {
        List<INetworkServerDelegate> _delegates = new List<INetworkServerDelegate>();

        NetworkServerSimple _server;
        int _port;

        public const int DefaultPort = 8888;

        public UnetNetworkServer(int port=DefaultPort, HostTopology topology=null)
        {
            _port = port;
            _server = new NetworkServerSimple();
            if(topology != null)
            {
                _server.Configure(topology);
            }
            RegisterHandlers();
        }

        public void Dispose()
        {
            Stop();
            UnregisterHandlers();
            _server = null;
            _delegates.Clear();
            _delegates = null;
        }

        public void Start()
        {
            _server.Listen(_port);
            for(var i = 0; i < _delegates.Count; i++)
            {                
                _delegates[i].OnStarted();
            }
        }

        public void Stop()
        {
            _server.Stop();
            for(var i = 0; i < _delegates.Count; i++)
            {                
                _delegates[i].OnStopped();
            }
        }

        void RegisterHandlers()
        {
            UnregisterHandlers();
            _server.RegisterHandler(MsgType.Connect, OnConnectReceived);
            _server.RegisterHandler(MsgType.Disconnect, OnDisconnectReceived);
            _server.RegisterHandler(MsgType.Error, OnErrorReceived);
            for(byte i = MsgType.Highest + 1; i < byte.MaxValue; i++)
            {
                _server.RegisterHandler(i, OnMessageReceived);
            }
        }

        void UnregisterHandlers()
        {
            _server.UnregisterHandler(MsgType.Connect);
            _server.UnregisterHandler(MsgType.Disconnect);
            _server.UnregisterHandler(MsgType.Error);
            for(byte i = MsgType.Highest + 1; i < byte.MaxValue; i++)
            {
                _server.RegisterHandler(i, OnMessageReceived);
            }
        }

        void OnConnectReceived(NetworkMessage umsg)
        {
            var clientId = (byte)umsg.conn.connectionId;
            for(var i = 0; i < _delegates.Count; i++)
            {                
                _delegates[i].OnClientConnected(clientId);
            }
        }

        void OnDisconnectReceived(NetworkMessage umsg)
        {
            var clientId = (byte)umsg.conn.connectionId;
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientDisconnected(clientId);
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
            var clientId = (byte)umsg.conn.connectionId;
            byte type = UnetNetworkMessage.ConvertType(umsg.msgType);
            var msg = new ReceivedNetworkMessage(type, umsg.channelId, new UnetNetworkReader(umsg.reader));
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnMessageReceived(clientId, msg);
            }
        }

        public INetworkMessage CreateMessage(byte type, int channelId)
        {
            var conns = new NetworkConnection[_server.connections.Count];
            _server.connections.CopyTo(conns, 0);
            return new UnetNetworkMessage(conns, type, channelId);
        }

        public INetworkMessage CreateMessage(byte clientId, byte type, int channelId)
        {
            var conn = _server.FindConnection(clientId);
            if(conn == null)
            {
                throw new InvalidOperationException("Could not find client id.");
            }
            return new UnetNetworkMessage(new NetworkConnection[]{conn}, type, channelId);
        }

        public void AddDelegate(INetworkServerDelegate dlg)
        {
            _delegates.Add(dlg);
        }

        public void RemoveDelegate(INetworkServerDelegate dlg)
        {
            _delegates.Remove(dlg);
        }
    }
}