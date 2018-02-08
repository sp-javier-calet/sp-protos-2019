using System;
using System.Collections.Generic;
using System.Net.Sockets;
using SocialPoint.Base;
using SocialPoint.IO;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class TcpSocketNetworkClient : INetworkClient, IDisposable, IUpdateable
    {
        List<INetworkClientDelegate> _delegates = new List<INetworkClientDelegate>();
        INetworkMessageReceiver _receiver;
        string _serverAddr;
        int _serverPort;
        TcpClient _client;
        bool _connecting;
        bool _connected;
        IUpdateScheduler _scheduler;
        List<NetworkStream> _stream;
        TcpSocketMessageReader _socketMessageReader;

        public TcpSocketNetworkClient(IUpdateScheduler scheduler, string serverAddr = TcpSocketNetworkServer.DefaultAddress, int serverPort = TcpSocketNetworkServer.DefaultPort)
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
            _stream = new List<NetworkStream>();
            _stream.Add(_client.GetStream());
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
            return new TcpSocketNetworkMessage(data, _stream);
        }

        public void Update()
        {
            ConnectClient();
            ReceiveServerMessages();
            DisconnectClient();
        }

        void ConnectClient()
        {
            if(_connecting && _client.Connected)
            {
                _connecting = false;
                _connected = true;
                for(var i = 0; i < _delegates.Count; i++)
                {
                    _delegates[i].OnClientConnected();
                }

                _socketMessageReader = new TcpSocketMessageReader(_client.GetStream());
                _socketMessageReader.MessageReceived += OnServerMessageReceived;
            }
        }

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

        void DisconnectClient()
        {
            if(Connected && IsSocketDisconnected())
            {
                OnDisconnected();
            }
        }

        bool IsSocketDisconnected()
        {
            if(_client.Client.Poll(0, SelectMode.SelectRead))
            {
                byte[] buff = new byte[1];
                if(_client.Client.Receive(buff, SocketFlags.Peek) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        void ReceiveServerMessages()
        {
            while(_client.Connected && _client.Available > 0)
            {
                _socketMessageReader.Receive();
            }
        }

        void OnDisconnected()
        {
            _connected = false;
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientDisconnected();
            }
            _socketMessageReader.MessageReceived -= OnServerMessageReceived;
            _scheduler.Remove(this);
        }

        public void Dispose()
        {
            DisconnectClient();
            _delegates.Clear();
            _delegates = null;
            _receiver = null;
            _stream.Clear();
            _stream = null;
        }
    }
}