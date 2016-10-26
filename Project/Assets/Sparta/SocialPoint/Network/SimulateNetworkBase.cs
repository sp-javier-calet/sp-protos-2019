using SocialPoint.Base;
using System.Collections.Generic;
using System;
using System.IO;
using SocialPoint.IO;

namespace SocialPoint.Network
{
    public class SimulateNetworkBase : INetworkMessageReceiver, INetworkMessageSender
    {
        class MessageInfo
        {
            public NetworkMessageData Data;
            public byte[] Body;
            public float Time;
        }

        class NetworkMessage : INetworkMessage
        {
            SimulateNetworkBase _sim;
            NetworkMessageData _data;
            MemoryStream _stream;

            public IWriter Writer{ get; private set; }

            public NetworkMessage(NetworkMessageData data, SimulateNetworkBase sim)
            {
                _data = data;
                _sim = sim;
                _stream = new MemoryStream();
                Writer = new SystemBinaryWriter(_stream);
            }

            public void Send()
            {
                _sim.OnMessageSent(_data, _stream.ToArray());
            }
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

        bool _blockEmission;
        public bool BlockEmission
        {
            get
            {
                return _blockEmission;
            }

            set
            {
                _blockEmission = value;
                if(!_blockEmission)
                {
                    for(var i=0; i<_sentMessages.Count; i++)
                    {
                        var msg = _sentMessages[i];
                        OnMessageSent(msg.Data, msg.Body);
                    }
                    _sentMessages.Clear();
                }
            }
        }

        List<MessageInfo> _receivedMessages;
        List<MessageInfo> _sentMessages;
        INetworkMessageSender _sender;
        List<INetworkClientDelegate> _delegates;
        INetworkMessageReceiver _receiver;
        float _time;

        public SimulateNetworkBase(INetworkMessageSender sender)
        {
            _sender = sender;
            _receivedMessages = new List<MessageInfo>();
            _sentMessages = new List<MessageInfo>();
        }

        public void ClearSimulationData()
        {
            _time = 0.0f;
            _receivedMessages.Clear();
            _sentMessages.Clear();
        }

        public INetworkMessage CreateMessage(NetworkMessageData data)
        {
            if(_blockEmission)
            {
                return new NetworkMessage(data, this);
            }
            return _sender.CreateMessage(data);
        }

        void OnMessageSent(NetworkMessageData data, byte[] body)
        {
            if(_blockEmission)
            {
                _sentMessages.Add(new MessageInfo{
                    Data = data,
                    Body = body,
                    Time = _time
                });
                return;
            }
            _sender.SendMessage(data, body);
        }

        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        public void Update(float dt)
        {
            _time += dt;
        }

        void INetworkMessageReceiver.OnMessageReceived(NetworkMessageData data, IReader reader)
        {
            if(BlockReception)
            {
                _receivedMessages.Add(new MessageInfo{
                    Data = data,
                    Body = reader.ReadBytes(int.MaxValue),
                    Time = _time
                });
                return;
            }
            OnMessageReceived(data, reader);
        }

        virtual protected void OnMessageReceived(NetworkMessageData data, IReader reader)
        {
            if(_receiver != null)
            {
                _receiver.OnMessageReceived(data, reader);
            }
        }

    }
}
