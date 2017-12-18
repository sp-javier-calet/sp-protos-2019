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
    public interface ISimpleSocketNetworkServer : INetworkServer
    {
        //        byte OnClientConnecting(LocalNetworkClient client);
        void OnClientConnected(SimpleSocketNetworkClient client);

        void OnClientDisconnected(SimpleSocketNetworkClient client);
        //        void OnLocalMessageReceived(LocalNetworkClient origin, ILocalNetworkMessage msg);
        //        ILocalNetworkMessage CreateLocalMessage(NetworkMessageData data);
    }

    public class SimpleSocketNetworkServer : ISimpleSocketNetworkServer, IDisposable, IUpdateable
    {
        public const int DefaultPort = 8888;

        private INetworkMessageReceiver _receiver;
        private List<INetworkServerDelegate> _delegates = new List<INetworkServerDelegate>();
        private IUpdateScheduler _updateScheduler;
        private TcpListener _listener;
        private List<TcpClient> _connectedClients = new List<TcpClient>();
        private List<TcpClient> _disconnectedClients = new List<TcpClient>();
        private List<SimpleSocketClientMessageData> _clientMesages = new List<SimpleSocketClientMessageData>();

        List<SimpleSocketNetworkClient> _networkClientList = new List<SimpleSocketNetworkClient>();

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
            foreach(var client in _networkClientList)
            {
                client.OnServerStarted();
            }
            _updateScheduler.Add(this);

        }

        public void Stop()
        {
            foreach(var client in _connectedClients)
            {
                for(var i = 0; i < _delegates.Count; i++)
                {
                    _delegates[i].OnClientDisconnected((byte)(i + 1));
                }
            }

            foreach(var client in _networkClientList)
            {
                client.OnServerStopped();
            }

            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnServerStopped();
            }


            _listener.Stop();

            _updateScheduler.Remove(this);

        }

        public void OnClientConnected(SimpleSocketNetworkClient client)
        {
            if(!_networkClientList.Contains(client))
            {
                _networkClientList.Add(client);
            }
        }

        public void OnClientDisconnected(SimpleSocketNetworkClient client)
        {
            if(_networkClientList.Contains(client))
            {
                _networkClientList.Remove(client);
            }
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
            throw new NotImplementedException();
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
            for(var i = 0; i < _connectedClients.Count; i++)
            {
                var c = _connectedClients[i];
                while (c.Available > 0 && c.Connected)
                {
                    _clientMesages[i].Receive(c.Client);
                }
            }
        }

        void ConnectClients()
        {
            if(_listener.Pending())
            {
                var newClient = _listener.AcceptTcpClient();
                _connectedClients.Add(newClient);
                byte clienId = (byte)_connectedClients.Count;
                var msg = new SimpleSocketClientMessageData(clienId);
                msg.MessageReceived += OnClientMessageReceived;
                _clientMesages.Add(msg);
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
            foreach(var c in _connectedClients)
            {
                if(IsSocketConnected(c.Client) == false)
                {
                    _disconnectedClients.Add(c);
                }
            }

            if(_disconnectedClients.Count > 0)
            {
                var disconnectedClients = _disconnectedClients.ToArray();
                _disconnectedClients.Clear();
                foreach(var client in disconnectedClients)
                {
                    for(var i = 0; i < _delegates.Count; i++)
                    {
                        _delegates[i].OnClientDisconnected((byte)(_connectedClients.IndexOf(client) + 1));
                    }
                    int posClient = _connectedClients.IndexOf(client);
                    _connectedClients.RemoveAt(posClient);
                    _clientMesages.RemoveAt(posClient);
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
