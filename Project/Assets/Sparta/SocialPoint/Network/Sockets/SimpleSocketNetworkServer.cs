using System.Collections.Generic;
using System;
using SocialPoint.Base;
using SocialPoint.Utils;
using System.Net;
using System.Net.Sockets;

namespace SocialPoint.Network
{
    public class SimpleSocketNetworkServer : INetworkServer, IDisposable, IUpdateable
    {
        public const int DefaultPort = 8888;

        //private INetworkMessageReceiver _receiver;
        private List<INetworkServerDelegate> _delegates = new List<INetworkServerDelegate>();
        private IUpdateScheduler _updateScheduler;
        private TcpListener _listener;
        private List<TcpClient> _connectedClients = new List<TcpClient>();

        public SimpleSocketNetworkServer(IUpdateScheduler updateScheduler, string serverAddr = null, int port = DefaultPort)
        {
            _updateScheduler = updateScheduler;
            _listener = new TcpListener(IPAddress.Parse(serverAddr), port);
        }

        public void Start()
        {
            _listener.Start();
            _updateScheduler.Add(this);
        }

        public void Stop()
        {
            _updateScheduler.Remove(this);
        }

        public void Fail(Error err)
        {
            throw new NotImplementedException();
        }

        public void AddDelegate(INetworkServerDelegate dlg)
        {
            UnityEngine.Debug.Log("SocketNetworkServer AddDelegate");
            _delegates.Add(dlg);
            if(Running && dlg != null)
            {
                dlg.OnServerStarted();
            }
        }

        public void RemoveDelegate(INetworkServerDelegate dlg)
        {
            UnityEngine.Debug.Log("SocketNetworkServer RemoveDelegate");
            _delegates.Remove(dlg);
        }

        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            UnityEngine.Debug.Log("SocketNetworkServer RegisterReceiver");
            //_receiver = receiver;
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
            //_receiver = null;
        }


        public void Update()
        {
            if (_listener.Pending())
            {
                var newClient = _listener.AcceptTcpClient();
                _connectedClients.Add(newClient);
                for(var i = 0; i < _delegates.Count; i++)
                {
                    _delegates[i].OnClientConnected((byte)_connectedClients.Count);
                }
            }
        }
    }
}
