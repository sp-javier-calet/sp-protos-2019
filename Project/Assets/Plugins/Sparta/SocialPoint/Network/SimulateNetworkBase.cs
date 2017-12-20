using SocialPoint.Base;
using System.Collections.Generic;
using System;
using System.IO;
using SocialPoint.IO;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class SimulateNetworkBase : INetworkMessageReceiver, IMemoryNetworkMessageReceiver, INetworkMessageSender, IDeltaUpdateable
    {
        class MessageInfo
        {
            public float Timestamp;
            public NetworkMessageData Data;
            public byte[] Body;
        }

        public float ReceptionDelay;
        public float ReceptionDelayVariance;
        public float EmissionDelay;
        public float EmissionDelayVariance;

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
                UpdatePendingMessages();
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
                UpdatePendingMessages();
            }
        }

        Queue<MessageInfo> _receivedMessages;
        Queue<MessageInfo> _sentMessages;
        INetworkMessageSender _sender;
        INetworkMessageReceiver _receiver;
        float _timestamp;
        float _lastReliableEmissionTimestamp;
        float _lastReliableReceptionTimestamp;

        public SimulateNetworkBase(INetworkMessageSender sender)
        {
            _sender = sender;
            _receivedMessages = new Queue<MessageInfo>();
            _sentMessages = new Queue<MessageInfo>();
        }

        public void ClearSimulationData()
        {
            _receivedMessages.Clear();
            _sentMessages.Clear();
            _timestamp = 0.0f;
            _lastReliableEmissionTimestamp = 0.0f;
            _lastReliableReceptionTimestamp = 0.0f;
        }

        public INetworkMessage CreateMessage(NetworkMessageData data)
        {
            if(_blockEmission || _sender == null)
            {
                return new MemoryNetworkMessage(data, this);
            }
            return _sender.CreateMessage(data);
        }

        public void Update(float dt)
        {
            _timestamp += dt;
            UpdatePendingMessages();
        }

        void UpdatePendingMessages()
        {
            if(!BlockEmission)
            {
                while(_sentMessages.Count > 0)
                {
                    var msg = _sentMessages.Peek();
                    if(msg.Timestamp > _timestamp)
                    {
                        break;
                    }
                    SendNextMessage();
                }
            }
            if(!BlockReception)
            {
                while(_receivedMessages.Count > 0)
                {
                    var msg = _receivedMessages.Peek();
                    if(msg.Timestamp > _timestamp)
                    {
                        break;
                    }
                    ReceiveNextMessage();
                }
            }
        }

        void IMemoryNetworkMessageReceiver.OnMessageSent(NetworkMessageData data, byte[] body)
        {
            var endTimestamp = _timestamp + RandomEmissionDelay;
            if(!data.Unreliable)
            {
                endTimestamp = Math.Max(endTimestamp, _lastReliableEmissionTimestamp);
                _lastReliableEmissionTimestamp = endTimestamp;
            }
            if(!_blockEmission && endTimestamp <= _timestamp)
            {
                SendMessage(data, body);
                return;
            }
            _sentMessages.Enqueue(new MessageInfo {
                Timestamp = endTimestamp,
                Data = data,
                Body = body
            });
        }

        void SendMessage(NetworkMessageData data, byte[] body)
        {
            if(_sender != null)
            {
                _sender.SendMessage(data, body);
            }
        }

        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        float GetRandom(float mean, float vari)
        {
            var val = mean;
            if(vari > 0.0f)
            {
                val += RandomUtils.Range(-vari, +vari);
            }
            return val;
        }

        float RandomReceptionDelay
        {
            get
            {
                var delay = GetRandom(ReceptionDelay, ReceptionDelayVariance);
                return delay < 0.0f ? 0.0f : delay;
            }
        }


        float RandomEmissionDelay
        {
            get
            {
                var delay = GetRandom(EmissionDelay, EmissionDelayVariance);
                return delay < 0.0f ? 0.0f : delay;
            }
        }

        void INetworkMessageReceiver.OnMessageReceived(NetworkMessageData data, IReader reader)
        {
            var endTimestamp = _timestamp + RandomReceptionDelay;
            if(!data.Unreliable)
            {
                endTimestamp = Math.Max(endTimestamp, _lastReliableReceptionTimestamp);
                _lastReliableReceptionTimestamp = endTimestamp;
            }
            if(!_blockReception && endTimestamp <= _timestamp)
            {
                ReceiveMessage(data, reader);
                return;
            }
            _receivedMessages.Enqueue(new MessageInfo {
                Timestamp = endTimestamp,
                Data = data,
                Body = reader.ReadCompleteByteArray()
            });
            return;
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
            var msg = _receivedMessages.Dequeue();
            ReceiveMessage(msg.Data, msg.Body);
            return true;
        }

        public bool SendNextMessage()
        {
            if(_sentMessages.Count == 0)
            {
                return false;
            }
            var msg = _sentMessages.Dequeue();
            SendMessage(msg.Data, msg.Body);
            return true;
        }

    }
}
