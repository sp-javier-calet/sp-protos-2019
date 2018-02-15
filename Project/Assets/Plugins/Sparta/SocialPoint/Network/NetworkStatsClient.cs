using System;
using System.Collections.Generic;
using SocialPoint.IO;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class NetworkStatsClient : NetworkStatsBase, INetworkClient, INetworkClientDelegate, IUpdateable
    {
        INetworkClient _client;
        IUpdateScheduler _scheduler;
        List<INetworkClientDelegate> _delegates;
        List<int> _latencies;

        public int PingInterval = NetworkStatsServer.DefaultSendStatusMessageInterval;

        public NetworkStatsClient(INetworkClient client, IUpdateScheduler scheduler) :
            base(client)
        {
            _delegates = new List<INetworkClientDelegate>();
            _client = client;
            _client.RegisterReceiver(this);
            _client.AddDelegate(this);
            _latencies = new List<int>();
            _scheduler = scheduler;
            if(_client.LatencySupported)
            {
                _scheduler.Add(this, UpdateableTimeMode.GameTimeUnscaled, PingInterval);
            }
        }

        #region IUpdateable implementation

        public void Update()
        {
            var delay = Latency;
            var pos = _latencies.FindLastIndex(l => l < delay);
            _latencies.Insert(pos + 1, delay);
        }

        #endregion

        override public void OnMessageReceived(NetworkMessageData data, IReader reader)
        {
            switch(data.MessageType)
            {
            case LatencyMessageType:
                OnLatencyMessageReceived(reader);
                break;
            }
            base.OnMessageReceived(data, reader);
        }

        void OnLatencyMessageReceived(IReader reader)
        {
            var msg = new NetworkLatencyMessage();
            msg.Deserialize(reader);
            var delay = GetDelay(msg.Timestamp);
            var pos = _latencies.FindLastIndex(l => l < delay);
            _latencies.Insert(pos + 1, delay);
        }

        public int LowestLatency
        {
            get
            {
                return _latencies.Count > 0 ? _latencies[0] : -1;
            }
        }

        public int HighestLatency
        {
            get
            {
                return _latencies.Count > 0 ? _latencies[_latencies.Count - 1] : -1;
            }
        }

        public int AverageLatency
        {
            get
            {
                var sum = 0;
                for(int i = 0; i < _latencies.Count; i++)
                {
                    sum += _latencies[i];
                }
                return _latencies.Count > 0 ? sum / _latencies.Count : -1;
            }
        }

        #region INetworkClient implementation

        public void Connect()
        {
            RestartStats();
            if(_client.LatencySupported)
            {
                _scheduler.Add(this, UpdateableTimeMode.GameTimeUnscaled, PingInterval);
            }
            _client.Connect();
        }

        public void Disconnect()
        {
            if(_client.LatencySupported)
            {
                _scheduler.Remove(this);
            }
            _client.Disconnect();
        }

        public void AddDelegate(INetworkClientDelegate dlg)
        {
            _delegates.Add(dlg);
        }

        public void RemoveDelegate(INetworkClientDelegate dlg)
        {
            _delegates.Remove(dlg);
        }

        public int GetDelay(int networkTimestamp)
        {
            return _client.GetDelay(networkTimestamp);
        }

        public bool Connected
        {
            get
            {
                return _client.Connected;
            }
        }

        public bool LatencySupported
        {
            get
            {
                return _client.LatencySupported;
            }
        }

        public int Latency
        {
            get
            {
                return _client.Latency;
            }
        }

        #endregion

        #region INetworkClientDelegate implementation

        public void OnClientConnected()
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientConnected();
            }
        }

        public void OnClientDisconnected()
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientDisconnected();
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

        protected override void RestartStats()
        {
            base.RestartStats();
            _latencies = new List<int>();
        }

        void IDisposable.Dispose()
        {
            Disconnect();
            _scheduler.Remove(this);
            _client.RemoveDelegate(this);
            _client.Dispose();
            _latencies.Clear();
            _delegates.Clear();
        }
    }
}

