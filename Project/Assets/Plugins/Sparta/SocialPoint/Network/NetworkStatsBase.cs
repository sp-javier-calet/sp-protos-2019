using System;
using System.IO;
using SocialPoint.IO;
using System.Collections.Generic;

namespace SocialPoint.Network
{
    public class NetworkStatsBase : INetworkMessageReceiver, INetworkMessageSender
    {
        public class NetworkLatencyMessage : INetworkShareable
        {
            public int Timestamp { get; private set; }

            public NetworkLatencyMessage(int timestamp = 0)
            {
                Timestamp = timestamp;
            }

            #region INetworkShareable implementation

            public void Deserialize(IReader reader)
            {
                Timestamp = reader.ReadInt32();
            }

            public void Serialize(IWriter writer)
            {
                writer.Write(Timestamp);
            }

            #endregion
            
        }

        INetworkMessageSender _sender;
        INetworkMessageReceiver _receiver;

        int _uploadBandwidth;
        int _downloadBandwidth;

        protected const byte LatencyMessageType = 99;

        public NetworkStatsBase(INetworkMessageSender sender)
        {
            _sender = sender;
        }

        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        public void OnMessageSent(int dataLength)
        {
            _uploadBandwidth += dataLength;
        }

        #region INetworkMessageReceiver implementation

        virtual public void OnMessageReceived(NetworkMessageData data, IReader reader)
        {
            ReceiveMessage(data, reader);
        }

        #endregion

        virtual protected void ReceiveMessage(NetworkMessageData data, IReader reader)
        {
            _downloadBandwidth += data.MessageLength;
            if(_receiver != null)
            {
                _receiver.OnMessageReceived(data, reader);
            }
        }

        #region INetworkMessageSender implementation

        public INetworkMessage CreateMessage(NetworkMessageData data)
        {
            return new NetworkStatsMessage(_sender.CreateMessage(data), this);
        }

        #endregion

        public int DownloadBandwith
        {
            get
            {
                return _downloadBandwidth;
            }
        }

        public int UploadBandwith
        {
            get
            {
                return _uploadBandwidth;
            }
        }

        protected virtual void RestartStats()
        {
            _downloadBandwidth = 0;
            _uploadBandwidth = 0;
        }
    }

    class NetworkStatsMessage : INetworkMessage
    {
        readonly NetworkStatsWriter _writer;
        NetworkStatsBase _stats;
        INetworkMessage _realMsg;

        public NetworkStatsMessage(INetworkMessage realMsg, NetworkStatsBase stats)
        {
            _stats = stats;
            _realMsg = realMsg;
            _writer = new NetworkStatsWriter(_realMsg.Writer);
        }

        #region INetworkMessage implementation
        public void Send()
        {
            _realMsg.Send();
            _stats.OnMessageSent(_writer.DataLength);
        }
        public IWriter Writer
        {
            get
            {
                return _writer;
            }
        }
        #endregion
    }
}

