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
   
    public class SimpleSocketNetworkServer : INetworkServer, IDisposable, IUpdateable
    {
        public const int DefaultPort = 8888;

        private INetworkMessageReceiver _receiver;
        private List<INetworkServerDelegate> _delegates = new List<INetworkServerDelegate>();
        private IUpdateScheduler _updateScheduler;
        private TcpListener _listener;
        private List<SimpleSocketClientData> _connectedDataClients = new List<SimpleSocketClientData>();
        private List<SimpleSocketClientData> _disconnectedDataClients = new List<SimpleSocketClientData>();
        //        private List<SimpleSocketClientData> _clientData = new List<SimpleSocketClientData>();

        //        List<SimpleSocketNetworkClient> _networkClientList = new List<SimpleSocketNetworkClient>();

        public SimpleSocketNetworkServer(IUpdateScheduler updateScheduler, string serverAddr = null, int port = DefaultPort)
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
            foreach(var client in _connectedDataClients)
            {
                for(var i = 0; i < _delegates.Count; i++)
                {
                    _delegates[i].OnClientDisconnected(client.ClientId);
                }
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
            throw new NotImplementedException();
        }

        public bool Running{ get; protected set; }

        public string Id
        {
            get
            {
                throw new NotImplementedException();
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
            if(data.ClientIds != null && data.ClientIds.Count == 1)
            {
                foreach(var clientIdConnected in _connectedDataClients)
                {
                    if(data.ClientIds.Contains(clientIdConnected.ClientId))
                    {
                        clientsToSendMessage.Add(clientIdConnected.Stream);
                    }
                }
            }
            else
            {
                foreach(var simpleSocketClientData in _connectedDataClients)
                {
                    clientsToSendMessage.Add(simpleSocketClientData.Stream);
                }
            }
           
            return new SimpleSocketNetworkMessage(data, clientsToSendMessage);
        }

        public void Dispose()
        {
            _delegates.Clear();
            _delegates = null;
            _receiver = null;
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
                    c.Receive();
                }
            }
        }

        void ConnectClients()
        {
            if(_listener.Pending())
            {
                byte clienId = (byte)(_connectedDataClients.Count+1);
                var newClient = _listener.AcceptTcpClient();
                var data = new SimpleSocketClientData(clienId, newClient);
                _connectedDataClients.Add(data);
                data.MessageReceived += OnClientMessageReceived;
                for(var i = 0; i < _delegates.Count; i++)
                {
                    _delegates[i].OnClientConnected(clienId);
                }
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
            foreach(var c in _connectedDataClients)
            {
                if(IsSocketConnected(c.Client.Client) == false)
                {
                    _disconnectedDataClients.Add(c);
                }
            }

            if(_disconnectedDataClients.Count > 0)
            {
                var disconnectedClients = _disconnectedDataClients.ToArray();
                _disconnectedDataClients.Clear();
                foreach(var client in disconnectedClients)
                {
                    for(var i = 0; i < _delegates.Count; i++)
                    {
                        _delegates[i].OnClientDisconnected((byte)(client.ClientId));
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
    }
}
