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
        public struct ClientData
        {
            public TcpClient Client;
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
        List<ClientData> _disconnectedDataClients = new List<ClientData>();
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
                var client = _connectedDataClients[i];
                for(var j = 0; j < _delegates.Count; j++)
                {
                    _delegates[j].OnClientDisconnected(client.Id);
                }
                client.Client.Close();
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
            throw new NotImplementedException();
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

        public bool Running{ get; protected set; }

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
            var clientsToSendMessage = new  List<NetworkStream>();
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
                while(c.Client.Available > 0 && c.Client.Connected)
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
                clientData.Client = newTcpClient;
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
            for(int i = 0; i < _connectedDataClients.Count; i++)
            {
                var c = _connectedDataClients[i];
                if(IsSocketConnected(c.Client.Client) == false)
                {
                    _disconnectedDataClients.Add(c);
                }
            }

            if(_disconnectedDataClients.Count > 0)
            {
                var disconnectedClients = _disconnectedDataClients.ToArray();
                _disconnectedDataClients.Clear();
                for(int i = 0; i < disconnectedClients.Length; i++)
                {
                    var client = disconnectedClients[i];
                    for(var j = 0; j < _delegates.Count; j++)
                    {
                        _delegates[j].OnClientDisconnected((client.Id));
                    }
                    _connectedDataClients.Remove(client);
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
            _delegates.Clear();
            _delegates = null;
            _receiver = null;
            _connectedDataClients.Clear();
            _connectedDataClients = null;
            _disconnectedDataClients.Clear();
            _disconnectedDataClients = null;
        }
    }
}
