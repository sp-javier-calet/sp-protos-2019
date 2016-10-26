using SocialPoint.Base;
using System.Collections.Generic;
using System;
using System.IO;
using SocialPoint.IO;

namespace SocialPoint.Network
{
    public sealed class SimulateNetworkClient : INetworkClient, INetworkClientDelegate, INetworkMessageReceiver
    {
        class Message
        {
            public NetworkMessageData Data;
            public byte[] Body;
        }

        bool _blockReception;
        public bool BlockReception
        {
            get
            {
                return _blockReception;
            }
            set
            {
                _blockReception = value;
                if(!_blockReception)
                {
                    for(var i=0; i<_receivedMessages.Count; i++)
                    {
                        var msg = _receivedMessages[i];
                        var reader = new SystemBinaryReader(new MemoryStream(msg.Body));
                        ((INetworkMessageReceiver)this).OnMessageReceived(msg.Data, reader);
                    }
                    _receivedMessages.Clear();
                }
            }
        }

        List<Message> _receivedMessages;

        INetworkClient _client;

        List<INetworkClientDelegate> _delegates;
        INetworkMessageReceiver _receiver;

        public SimulateNetworkClient(INetworkClient client)
        {
            _receivedMessages = new List<Message>();
            _delegates = new List<INetworkClientDelegate>();
            _client = client;
            _client.RegisterReceiver(this);
            _client.AddDelegate(this);
        }

        public SimulateNetworkClient(LocalNetworkServer server):
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
            _receivedMessages.Clear();
            _client.Connect();
        }

        public void Disconnect()
        {
            _receivedMessages.Clear();
            _client.Disconnect();
        }

        public INetworkMessage CreateMessage(NetworkMessageData data)
        {
            return _client.CreateMessage(data);
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
            return _client.GetDelay(networkTimestamp);
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

        void INetworkMessageReceiver.OnMessageReceived(NetworkMessageData data, IReader reader)
        {
            if(BlockReception)
            {
                _receivedMessages.Add(new Message{
                    Data = data,
                    Body = reader.ReadBytes(int.MaxValue)
                });
                return;
            }
            if(_receiver != null)
            {
                _receiver.OnMessageReceived(data, reader);
            }
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnMessageReceived(data);
            }
        }

        #endregion
    }
}
