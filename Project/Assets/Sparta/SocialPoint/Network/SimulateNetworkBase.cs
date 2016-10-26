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
                _sim.SendMessage(_data, _stream.ToArray());
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
                        ReceiveMessage(msg.Data, msg.Body);
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
                        SendMessage(msg.Data, msg.Body);
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

        public SimulateNetworkBase(INetworkMessageSender sender)
        {
            _sender = sender;
            _receivedMessages = new List<MessageInfo>();
            _sentMessages = new List<MessageInfo>();
        }

        public void ClearSimulationData()
        {
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

        void SendMessage(NetworkMessageData data, byte[] body)
        {
            if(_blockEmission)
            {
                _sentMessages.Add(new MessageInfo{
                    Data = data,
                    Body = body
                });
                return;
            }
            _sender.SendMessage(data, body);
        }

        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        void INetworkMessageReceiver.OnMessageReceived(NetworkMessageData data, IReader reader)
        {
            if(_blockReception)
            {
                _receivedMessages.Add(new MessageInfo{
                    Data = data,
                    Body = reader.ReadBytes(int.MaxValue)
                });
                return;
            }
            ReceiveMessage(data, reader);
        }

        virtual protected void ReceiveMessage(NetworkMessageData data, IReader reader)
        {
            if(_receiver != null)
            {
                _receiver.OnMessageReceived(data, reader);
            }
        }

        void ReceiveMessage(NetworkMessageData data, byte[] body)
        {
            var reader = new SystemBinaryReader(new MemoryStream(body));
            ReceiveMessage(data, reader);
        }

        public bool ReceiveNextMessage()
        {
            if(_receivedMessages.Count == 0)
            {
                return false;
            }
            var msg = _receivedMessages[0];
            _receivedMessages.RemoveAt(0);
            ReceiveMessage(msg.Data, msg.Body);
            return true;
        }

        public bool SendNextMessage()
        {
            if(_sentMessages.Count == 0)
            {
                return false;
            }
            var msg = _sentMessages[0];
            _sentMessages.RemoveAt(0);
            SendMessage(msg.Data, msg.Body);
            return true;
        }

    }
}
