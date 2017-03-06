using System;
using System.Collections.Generic;
using SocialPoint.IO;

namespace SocialPoint.Network
{
    public class NetworkStatsClient : NetworkStatsBase, INetworkClient, INetworkClientDelegate
    {
        INetworkClient _client;
        List<INetworkClientDelegate> _delegates;
        List<int> _latencies;

        public NetworkStatsClient(INetworkClient client) :
            base(client)
        {
            _delegates = new List<INetworkClientDelegate>();
            _client = client;
            _client.RegisterReceiver(this);
            _client.AddDelegate(this);
            _latencies = new List<int>();
        }

        override public void OnMessageReceived(NetworkMessageData data, IReader reader)
        {
            switch(data.MessageType)
            {
            case StatsMessageType:
                OnStatsMessageReceived(reader);
                break;
            }
            base.OnMessageReceived(data, reader);
        }

        void OnStatsMessageReceived(IReader reader)
        {
            var msg = new NetworkStatsMessage();
            msg.Deserialize(reader);
            _latencies.Add(GetDelay(msg.Timestamp));
            _latencies.Sort();
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
            _client.Connect();
        }

        public void Disconnect()
        {
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

        public byte ClientId
        {
            get
            {
                return _client.ClientId;
            }
        }

        public bool Connected
        {
            get
            {
                return _client.Connected;
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
    }
}

