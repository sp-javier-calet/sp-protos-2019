using System.Collections.Generic;
using System;
using SocialPoint.Base;
using SocialPoint.Utils;
using System.Net;
using System.Net.Sockets;
using SocialPoint.IO;
using System.IO;

namespace SocialPoint.Network
{
    public class TcpSocketNetworkServer : INetworkServer, IDisposable, IUpdateable
    {
        struct ClientData
        {
            public TcpClient TcpClient;
            public byte Id;
            public TcpSocketMessageReader Reader;
        }

        public const string DefaultAddress = "127.0.0.1";
        public const int DefaultPort = 8888;

        INetworkMessageReceiver _receiver;
        List<INetworkServerDelegate> _delegates = new List<INetworkServerDelegate>();
        IUpdateScheduler _updateScheduler;
        TcpListener _listener;
        List<ClientData> _connectedDataClients = new List<ClientData>();
        byte _nextClientID = 1;

        public TcpSocketNetworkServer(IUpdateScheduler updateScheduler, string serverAddr = DefaultAddress, int port = DefaultPort)
        {
            _updateScheduler = updateScheduler;
            _listener = new TcpListener(IPAddress.Parse(serverAddr), port);
        }

        public void Start()
        {
            _listener.Start();
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
                clientData.TcpClient.Client.Close();
                clientData.TcpClient.Close();
            }
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnServerStopped();
            }
            _listener.Stop();
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

        public INetworkMessage CreateMessage(NetworkMessageData data)
        {
            var clientsToSendMessage = new List<NetworkStream>();
            if(data.ClientIds != null && data.ClientIds.Count > 0)
            {
                for(int i = 0; i < _connectedDataClients.Count; i++)
                {
                    var clientData = _connectedDataClients[i];
                    if(data.ClientIds.Contains(clientData.Id))
                    {
                        clientsToSendMessage.Add(clientData.Reader.Stream);
                    }
                }
            }
            else
            {
                for(int i = 0; i < _connectedDataClients.Count; i++)
                {
                    var simpleSocketClientData = _connectedDataClients[i];
                    clientsToSendMessage.Add(simpleSocketClientData.Reader.Stream);
                }
            }

            return new TcpSocketNetworkMessage(data, clientsToSendMessage);
        }

        public void Update()
        {
            ConnectClients();

            DisconnectClients();

            ReceiveClientData();
        }

        void ReceiveClientData()
        {
            for(var i = 0; i < _connectedDataClients.Count; i++)
            {
                var c = _connectedDataClients[i];
                while(c.TcpClient.Available > 0 && c.TcpClient.Connected)
                {
                    c.Reader.Receive();
                }
            }
        }

        void ConnectClients()
        {
            while(_listener.Pending())
            {
                ClientData clientData = new ClientData();
                var newTcpClient = _listener.AcceptTcpClient();
                var messageReader = new TcpSocketMessageReader(newTcpClient.GetStream(), _nextClientID);
                clientData.TcpClient = newTcpClient;
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

        void DisconnectClients()
        {
            for(int i = _connectedDataClients.Count - 1; i >= 0; i--)
            {
                var c = _connectedDataClients[i];
                if(IsSocketConnected(c.TcpClient.Client) == false)
                {
                    c.TcpClient.Client.Close();
                    c.TcpClient.Close();

                    for(var j = 0; j < _delegates.Count; j++)
                    {
                        _delegates[j].OnClientDisconnected((c.Id));
                    }
                    _connectedDataClients.RemoveAt(i);
                }
            }
        }

        bool IsSocketConnected(Socket s)
        {
            // https://stackoverflow.com/questions/2661764/how-to-check-if-a-socket-is-connected-disconnected-in-c
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if((part1 && part2) || !s.Connected)
                return false;
            else
                return true;
        }

        public void Dispose()
        {
            _updateScheduler.Remove(this);
            _updateScheduler = null;
            _delegates.Clear();
            _delegates = null;
            _receiver = null;
            _connectedDataClients.Clear();
            _connectedDataClients = null;
            _listener = null;
        }
    }
}
