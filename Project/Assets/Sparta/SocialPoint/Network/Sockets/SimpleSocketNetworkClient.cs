using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Utils;
using System;
using System.Net.Sockets;

namespace SocialPoint.Network
{
    public class SimpleSocketNetworkClient : INetworkClient, IDisposable, IUpdateable
    {
        public enum Protocol
        {
            Tcp,
            Udp
        }

        public const string DefaultServerAddr = "localhost";

        private List<INetworkClientDelegate> _delegates = new List<INetworkClientDelegate>();
        //private INetworkMessageReceiver _receiver;
        private string _serverAddr;
        private int _serverPort;
        private TcpClient _client;
        private bool _connecting;
        private bool _connected;
        IUpdateScheduler _scheduler;
        NetworkStream _stream;

        public SimpleSocketNetworkClient(IUpdateScheduler scheduler,string serverAddr = null, int serverPort = UnetNetworkServer.DefaultPort)
        {
            _scheduler = scheduler;
            _serverAddr = serverAddr;
            _serverPort = serverPort;
            _client = new TcpClient();
        }

        public void Connect()
        {
            _connecting = true;
            _scheduler.Add(this);
            _client.Connect(_serverAddr, _serverPort);
            _stream = new NetworkStream(_client.Client);
        }

        public void Disconnect()
        {
            _client.Close();
            OnDisconnected();
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
            //_receiver = receiver;
        }

        public int GetDelay(int networkTimestamp)
        {
            throw new NotImplementedException();
        }

        public  byte ClientId
        {
            get
            {
                throw new NotImplementedException();
            }
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
            return new SimpleSocketNetworkMessage(data, _stream);
        }

        public void Update()
        {
            ConnectClients();
            DisconnectClients();
            ReceiveServertMessages();
        }

        void ConnectClients()
        {
            UnityEngine.Debug.Log("CLIENT Update ");
            if(_connecting && _client.Connected)
            {
                UnityEngine.Debug.Log("CLIENT OnClientConnected ");
                _connecting = false;
                _connected = true;
                for(var i = 0; i < _delegates.Count; i++)
                {
                    _delegates[i].OnClientConnected();
                }
            }
        }

        void DisconnectClients()
        {
            if(_connected && !_client.Connected)
            {
                UnityEngine.Debug.Log("CLIENT OnClientDisconnected ");
                OnDisconnected();
            }
        }

        void ReceiveServertMessages()
        {
        }

        void OnDisconnected()
        {
            _connected = false;
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientDisconnected();
            }
            _scheduler.Remove(this);
        }

        public void Dispose()
        {
            _delegates.Clear();
            _delegates = null;
            //_receiver = null;
        }


        public void OnServerStarted()
        {
            if(!Connected)
            {
                Connect();
            }
        }

        public void OnServerStopped()
        {
            if(Connected)
            {
                Disconnect();
            }
        }

        public void SendNetworkMessage(NetworkMessageData _data, string str)
        {
            if(_client != null && Connected)
            {
                //_client.GetStream().Write();
            }
            else
            {
                DebugUtils.Assert(false, "Message could not be sent. Socket is not connected");
            }
        }

    }
}