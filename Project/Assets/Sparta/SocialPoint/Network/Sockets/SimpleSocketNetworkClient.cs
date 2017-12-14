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
        IUpdateScheduler _scheduler;

        public SimpleSocketNetworkClient(IUpdateScheduler scheduler, string serverAddr = null, int serverPort = UnetNetworkServer.DefaultPort)
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
        }

        public void Disconnect()
        {
            _scheduler.Remove(this);
            _client.Close();
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
            throw new NotImplementedException();
        }

        public void Update()
        {
            if(_connecting && _client.Connected)
            {
                _connecting = false;
                for(var i = 0; i < _delegates.Count; i++)
                {
                    _delegates[i].OnClientConnected();
                }
            }
        }

        public void Dispose()
        {
            _delegates.Clear();
            _delegates = null;
            //_receiver = null;
        }
    }
}