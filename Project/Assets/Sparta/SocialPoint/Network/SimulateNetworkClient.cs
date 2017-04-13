﻿using SocialPoint.Base;
using System.Collections.Generic;
using System;
using System.IO;
using SocialPoint.IO;

namespace SocialPoint.Network
{
    public sealed class SimulateNetworkClient : SimulateNetworkBase, INetworkClient, INetworkClientDelegate
    {
        INetworkClient _client;
        List<INetworkClientDelegate> _delegates;

        public SimulateNetworkClient(INetworkClient client) :
            base(client)
        {
            _delegates = new List<INetworkClientDelegate>();
            _client = client;
            _client.RegisterReceiver(this);
            _client.AddDelegate(this);
        }

        public SimulateNetworkClient(LocalNetworkServer server) :
            this(new LocalNetworkClient(server))
        {
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

        public void Connect()
        {
            ClearSimulationData();
            _client.Connect();
        }

        public void Disconnect()
        {
            ClearSimulationData();
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

        #region INetworkClientDelegate implementation

        void INetworkClientDelegate.OnClientConnected()
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientConnected();
            }
        }

        void INetworkClientDelegate.OnClientDisconnected()
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientDisconnected();
            }
        }

        void INetworkClientDelegate.OnMessageReceived(NetworkMessageData data)
        {
        }

        void INetworkClientDelegate.OnNetworkError(Error err)
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnNetworkError(err);
            }
        }

        #endregion

        #region INetworkMessageReceiver implementation

        protected override void ReceiveMessage(NetworkMessageData data, IReader reader)
        {
            base.ReceiveMessage(data, reader);
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnMessageReceived(data);
            }
        }

        #endregion
    }
}
