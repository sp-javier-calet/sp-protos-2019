using System;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.IO;
using SocialPoint.Utils;
using LiteNetLib;
using LiteNetLib.Utils;

namespace SocialPoint.Network
{
    public class UdpSocketNetworkClient : INetworkClient, IDisposable, IUpdateable, INetEventListener
    {
        public string ServerAddress { get; set; }
        public int ServerPort { get; set; }

        List<INetworkClientDelegate> _delegates = new List<INetworkClientDelegate>();
        INetworkMessageReceiver _receiver;
        UdpSocketMessageReader _socketMessageReader;

        IUpdateScheduler _scheduler;
        NetManager _client;
        NetPeer _peer;
        bool _connected;

        public UdpSocketNetworkClient(IUpdateScheduler scheduler, string connectionKey = UdpSocketNetworkServer.DefaultConnectionKey, int updateTime = UdpSocketNetworkServer.DefaultUpdateTime)
        {
            ServerAddress = UdpSocketNetworkServer.DefaultAddress;
            ServerPort = UdpSocketNetworkServer.DefaultPort;

            _scheduler = scheduler;
            _client = new NetManager(this, connectionKey);
            _client.UpdateTime = updateTime;
        }

        public void Connect()
        {
            _client.Start();
            _peer = _client.Connect(ServerAddress, ServerPort);
            _scheduler.Add(this);
        }

        public void Disconnect()
        {
            _client.DisconnectPeer(_peer);
        }

        public void AddDelegate(INetworkClientDelegate dlg)
        {
            _delegates.Add(dlg);
        }

        public void RemoveDelegate(INetworkClientDelegate dlg)
        {
            _delegates.Remove(dlg);
        }

        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        public int GetDelay(int networkTimestamp)
        {
            return 0;
        }

        public bool Connected
        {
            get
            {
                return _connected;
            }
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

        public INetworkMessage CreateMessage(NetworkMessageData data)
        {
            var peers = new List<NetPeer>();
            peers.Add(_client.GetFirstPeer());
            return new UdpSocketNetworkMessage(data, peers);
        }

        public void Update()
        {
            if(_client != null && _client.IsRunning)
            {
                _client.PollEvents();
            }
        }

        #region INetEventListener implementation

        public void OnPeerConnected(NetPeer peer)
        {
            _connected = true;
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientConnected();
            }
            _socketMessageReader = new UdpSocketMessageReader();
            _socketMessageReader.MessageReceived += OnServerMessageReceived;
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            _connected = false;
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientDisconnected();
            }
            if(_socketMessageReader != null)
            {
                _socketMessageReader.MessageReceived -= OnServerMessageReceived;
            }
            _client.Stop();
            _scheduler.Remove(this);
        }

        public void OnNetworkError(NetEndPoint endPoint, int socketErrorCode)
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                Error error = new Error(socketErrorCode);
                _delegates[i].OnNetworkError(error);
            }
        }

        public void OnNetworkReceive(NetPeer peer, NetDataReader reader)
        {
            _socketMessageReader.Receive(reader);
        }

        public void OnNetworkReceiveUnconnected(NetEndPoint remoteEndPoint, NetDataReader reader, UnconnectedMessageType messageType)
        {
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
        }

        #endregion

        void OnServerMessageReceived(NetworkMessageData data, IReader reader)
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnMessageReceived(data);
            }
            if(_receiver != null)
            {
                _receiver.OnMessageReceived(data, reader);
            }
        }

        public void Dispose()
        {
            _client.Stop();
            _delegates.Clear();
            _delegates = null;
            _receiver = null;
        }
    }
}