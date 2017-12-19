using System;
using System.Collections.Generic;
using SocialPoint.IO;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class NetworkStatsServer : NetworkStatsBase, INetworkServer, INetworkServerDelegate, IUpdateable
    {

        INetworkServer _server;
        List<INetworkServerDelegate> _delegates;
        List<byte> _clients;
        IUpdateScheduler _scheduler;

        public const int DefaultSendStatusMessageInterval = 10;
        public int SendStatusMessageInterval = DefaultSendStatusMessageInterval;

        public NetworkStatsServer(INetworkServer server, IUpdateScheduler scheduler) :
            base(server)
        {
            _delegates = new List<INetworkServerDelegate>();
            _server = server;
            _server.RegisterReceiver(this);
            _server.AddDelegate(this);
            _scheduler = scheduler;
            _clients = new List<byte>();
        }

        public void Start()
        {
            if(_scheduler != null && !_server.LatencySupported)
            {
                _scheduler.Add(this, UpdateableTimeMode.GameTimeUnscaled, SendStatusMessageInterval);
            }
            _server.Start();
        }

        #region IUpdateable implementation

        public void Update()
        {
            _server.SendMessage(new NetworkMessageData {
                MessageType = LatencyMessageType,
                ClientIds = _clients
            }, new NetworkLatencyMessage(_server.GetTimestamp())
            );
        }

        #endregion

        public void Stop()
        {
            if(_scheduler != null)
            {
                _scheduler.Remove(this);
            }

            _server.Stop();
        }

        public void Fail(SocialPoint.Base.Error err)
        {
            _server.Fail(err);
        }

        public void AddDelegate(INetworkServerDelegate dlg)
        {
            _delegates.Add(dlg);
        }

        public void RemoveDelegate(INetworkServerDelegate dlg)
        {
            _delegates.Remove(dlg);
        }

        public int GetTimestamp()
        {
            return _server.GetTimestamp();
        }

        public bool LatencySupported
        {
            get
            {
                return _server.LatencySupported;
            }
        }

        public bool Running
        {
            get
            {
                return _server.Running;
            }
        }

        public string Id
        {
            get
            {
                return _server.Id;
            }
        }

        #region INetworkServerDelegate implementation

        public void OnServerStarted()
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnServerStarted();
            }
        }

        public void OnServerStopped()
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnServerStopped();
            }
        }

        public void OnClientConnected(byte clientId)
        {
            _clients.Add(clientId);
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientConnected(clientId);
            }
        }

        public void OnClientDisconnected(byte clientId)
        {
            _clients.Remove(clientId);
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientDisconnected(clientId);
            }
        }

        public void OnMessageReceived(NetworkMessageData data)
        {
        }

        public void OnNetworkError(SocialPoint.Base.Error err)
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnNetworkError(err);
            }
        }

        #endregion

        protected override void ReceiveMessage(NetworkMessageData data, IReader reader)
        {
            base.ReceiveMessage(data, reader);
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnMessageReceived(data);
            }
        }
    }
}

