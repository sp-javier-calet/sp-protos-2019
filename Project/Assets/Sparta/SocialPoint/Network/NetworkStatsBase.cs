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

        List<int> _uploadBandwidth;
        List<int> _downloadBandwidth;

        protected const byte LatencyMessageType = 99;

        public NetworkStatsBase(INetworkMessageSender sender)
        {
            _sender = sender;
            _uploadBandwidth = new List<int>();
            _downloadBandwidth = new List<int>();
        }

        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        public void OnMessageSent(NetworkMessageData data, byte[] body)
        {
            var pos = _uploadBandwidth.FindLastIndex(l => l < body.Length);
            _uploadBandwidth.Insert(pos + 1, body.Length);
            if(_sender != null)
            {
                _sender.SendMessage(data, body);
            }
        }

        #region INetworkMessageReceiver implementation

        virtual public void OnMessageReceived(NetworkMessageData data, IReader reader)
        {
            ReceiveMessage(data, reader);
        }

        #endregion

        virtual protected void ReceiveMessage(NetworkMessageData data, IReader reader)
        {
            var pos = _downloadBandwidth.FindLastIndex(ml => ml < data.MessageLength);
            _downloadBandwidth.Insert(pos + 1, data.MessageLength);
            if(_receiver != null)
            {
                _receiver.OnMessageReceived(data, reader);
            }
        }

        #region INetworkMessageSender implementation

        public INetworkMessage CreateMessage(NetworkMessageData data)
        {
            return new NetworkStatsMessage(data, this);
        }

        #endregion

        public int DownloadBandwith
        {
            get
            {
                var sum = 0;
                for(int i = 0; i < _downloadBandwidth.Count; i++)
                {
                    sum += _downloadBandwidth[i];
                }
                return _downloadBandwidth.Count > 0 ? sum : -1;
            }
        }

        public int LowestDownloadBandwith
        {
            get
            {
                return _downloadBandwidth.Count > 0 ? _downloadBandwidth[0] : -1;
            }
        }

        public int HighestDownloadBandwith
        {
            get
            {
                return _downloadBandwidth.Count > 0 ? _downloadBandwidth[_downloadBandwidth.Count - 1] : -1;
            }
        }

        public int AverageDownloadBandwith
        {
            get
            {
                return _downloadBandwidth.Count > 0 ? DownloadBandwith / _downloadBandwidth.Count : -1;
            }
        }

        public int UploadBandwith
        {
            get
            {
                var sum = 0;
                for(int i = 0; i < _uploadBandwidth.Count; i++)
                {
                    sum += _uploadBandwidth[i];
                }
                return sum;
            }
        }

        public int LowestUploadBandwith
        {
            get
            {
                return _uploadBandwidth.Count > 0 ? _uploadBandwidth[0] : -1;
            }
        }

        public int HighestUploadBandwith
        {
            get
            {
                return _uploadBandwidth.Count > 0 ? _uploadBandwidth[_uploadBandwidth.Count - 1] : -1;
            }
        }

        public int AverageUploadBandwith
        {
            get
            {
                return _uploadBandwidth.Count > 0 ? UploadBandwith / _uploadBandwidth.Count : -1;
            }
        }
    }

    class NetworkStatsMessage : INetworkMessage
    {
        NetworkStatsBase _stats;
        NetworkMessageData _data;
        MemoryStream _stream;
        SystemBinaryWriter _writer;


        public NetworkStatsMessage(NetworkMessageData data, NetworkStatsBase stats)
        {
            _stats = stats;
            _data = data;
            _stream = new MemoryStream();
            _writer = new SystemBinaryWriter(_stream);
        }

        #region INetworkMessage implementation
        public void Send()
        {
            _stats.OnMessageSent(_data, _stream.ToArray());
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

