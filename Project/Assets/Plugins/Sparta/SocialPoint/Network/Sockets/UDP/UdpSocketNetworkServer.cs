using System;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.IO;
using SocialPoint.Utils;
using LiteNetLib;
using LiteNetLib.Utils;

namespace SocialPoint.Network
{
    public class UdpSocketNetworkServer : INetworkServer, IDisposable, IUpdateable, INetEventListener
    {
        struct ClientData
        {
            public NetPeer Client;
            public byte Id;
            public UdpSocketMessageReader Reader;
        }

        public const string DefaultAddress = "127.0.0.1";
        public const int DefaultPort = 8888;
        public const int DefaultPeerLimit = 100;
        public const string DefaultConnectionKey = "TestConnectionKey";
        public const int DefaultUpdateTime = 10;

        public int Port { get; set; }

        INetworkMessageReceiver _receiver;
        List<INetworkServerDelegate> _delegates = new List<INetworkServerDelegate>();
        IUpdateScheduler _updateScheduler;
        List<ClientData> _connectedDataClients = new List<ClientData>();
        NetManager _server;
        byte _nextClientID = 1;

        public UdpSocketNetworkServer(IUpdateScheduler updateScheduler, int peerLimit = DefaultPeerLimit, string connectionKey = DefaultConnectionKey, int updateTime = DefaultUpdateTime)
        {
            Port = DefaultPort;
            _updateScheduler = updateScheduler;
            _server = new NetManager(this, peerLimit, connectionKey);
            _server.UpdateTime = updateTime;
        }

        public void Start()
        {
            _server.Start(Port);

            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnServerStarted();
            }
            _updateScheduler.Add(this);
        }

        public void Stop()
        {
            for(int i = 0; i < _connectedDataClients.Count; i++)
            {
                var clientData = _connectedDataClients[i];
                for(var j = 0; j < _delegates.Count; j++)
                {
                    _delegates[j].OnClientDisconnected(clientData.Id);
                }
                _server.DisconnectPeer(clientData.Client);
            }
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnServerStopped();
            }
            StopServer();
            _updateScheduler.Remove(this);
        }

        public void Fail(Error err)
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnNetworkError(err);
            }
        }

        public void AddDelegate(INetworkServerDelegate dlg)
        {
            _delegates.Add(dlg);
            if(Running && dlg != null)
            {
                dlg.OnServerStarted();
            }
        }

        public void RemoveDelegate(INetworkServerDelegate dlg)
        {
            _delegates.Remove(dlg);
        }

        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        public int GetTimestamp()
        {
            return (int)TimeUtils.Timestamp;
        }

        public bool Running { get; protected set; }

        public string Id
        {
            get
            {
                return null;
            }
        }

        public bool LatencySupported
        {
            get
            {
                return false;
            }
        }


        public void Update()
        {
            if(_server.IsRunning)
            {
                _server.PollEvents();
            }
        }

        #region INetEventListener implementation

        public void OnPeerConnected(NetPeer peer)
        {
            ClientData clientData = new ClientData();
            var messageReader = new UdpSocketMessageReader(_nextClientID);
            clientData.Client = peer;
            clientData.Id = _nextClientID;
            clientData.Reader = messageReader;
            _connectedDataClients.Add(clientData);
            messageReader.MessageReceived += OnClientMessageReceived;
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientConnected(_nextClientID);
            }
            _nextClientID++;
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            var pos = _connectedDataClients.FindIndex(x => x.Client == peer);
            if(pos > -1)
            {
                var disconectedClient = _connectedDataClients[pos];
                _server.DisconnectPeer(disconectedClient.Client);
                for(var j = 0; j < _delegates.Count; j++)
                {
                    _delegates[j].OnClientDisconnected((disconectedClient.Id));
                }
                disconectedClient.Reader.MessageReceived -= OnClientMessageReceived;
                _connectedDataClients.Remove(disconectedClient);
            }
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
            for(var i = 0; i < _connectedDataClients.Count; i++)
            {
                var c = _connectedDataClients[i];
                if(c.Client == peer)
                {
                    c.Reader.Receive(reader);
                }
            }
        }

        public void OnNetworkReceiveUnconnected(NetEndPoint remoteEndPoint, NetDataReader reader, UnconnectedMessageType messageType)
        {
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
        }


        #endregion

        #region Messages
        public INetworkMessage CreateMessage(NetworkMessageData data)
        {
            var clientsToSendMessage = new List<NetPeer>();
            if(data.ClientIds != null && data.ClientIds.Count > 0)
            {
                for(int i = 0; i < _connectedDataClients.Count; i++)
                {
                    var clientData = _connectedDataClients[i];
                    if(data.ClientIds.Contains(clientData.Id))
                    {
                        clientsToSendMessage.Add(clientData.Client);
                    }
                }
            }
            else
            {
                for(int i = 0; i < _connectedDataClients.Count; i++)
                {
                    var simpleSocketClientData = _connectedDataClients[i];
                    clientsToSendMessage.Add(simpleSocketClientData.Client);
                }
            }

            return new UdpSocketNetworkMessage(data, clientsToSendMessage);
        }

        void OnClientMessageReceived(NetworkMessageData data, IReader reader)
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
        #endregion

        void StopServer()
        {
            if(_server.IsRunning)
            {
                _server.Stop();
            }
        }

        public void Dispose()
        {
            StopServer();
            _delegates.Clear();
            _delegates = null;
            _receiver = null;
            _connectedDataClients.Clear();
            _connectedDataClients = null;

        }
    }
}
