﻿using System;
using SocialPoint.IO;
using System.Collections.Generic;

namespace SocialPoint.Network
{
    public class NetworkStatsBase : INetworkMessageReceiver, INetworkMessageSender
    {
        public class NetworkStatsMessage : INetworkShareable
        {
            public int Timestamp { get; private set; }

            public NetworkStatsMessage(int timestamp = 0)
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

        List<int> _uploadBandwith;
        List<int> _downloadBandwith;

        protected const byte StatsMessageType = 99;

        public NetworkStatsBase(INetworkMessageSender sender)
        {
            _sender = sender;
            _uploadBandwith = new List<int>();
            _downloadBandwith = new List<int>();
        }

        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        void SendMessage(NetworkMessageData data, byte[] body)
        {
            var pos = _uploadBandwith.FindLastIndex(l => l < body.Length);
            _uploadBandwith.Insert(pos + 1, body.Length);
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
            var pos = _downloadBandwith.FindLastIndex(ml => ml < data.MessageLength);
            _downloadBandwith.Insert(pos + 1, data.MessageLength);
            if(_receiver != null)
            {
                _receiver.OnMessageReceived(data, reader);
            }
        }

        #region INetworkMessageSender implementation

        public INetworkMessage CreateMessage(NetworkMessageData data)
        {
            return _sender.CreateMessage(data);
        }

        #endregion

        public int DownloadBandwith
        {
            get
            {
                var sum = 0;
                for(int i = 0; i < _downloadBandwith.Count; i++)
                {
                    sum += _downloadBandwith[i];
                }
                return _downloadBandwith.Count > 0 ? sum : -1;
            }
        }

        public int LowestDownloadBandwith
        {
            get
            {
                return _downloadBandwith.Count > 0 ? _downloadBandwith[0] : -1;
            }
        }

        public int HighestDownloadBandwith
        {
            get
            {
                return _downloadBandwith.Count > 0 ? _downloadBandwith[_downloadBandwith.Count - 1] : -1;
            }
        }

        public int AverageDownloadBandwith
        {
            get
            {
                return _downloadBandwith.Count > 0 ? DownloadBandwith / _downloadBandwith.Count : -1;
            }
        }

        public int UploadBandwith
        {
            get
            {
                var sum = 0;
                for(int i = 0; i < _uploadBandwith.Count; i++)
                {
                    sum += _uploadBandwith[i];
                }
                return sum;
            }
        }

        public int LowestUploadBandwith
        {
            get
            {
                return _uploadBandwith.Count > 0 ? _uploadBandwith[0] : -1;
            }
        }

        public int HighestUploadBandwith
        {
            get
            {
                return _uploadBandwith.Count > 0 ? _uploadBandwith[_uploadBandwith.Count - 1] : -1;
            }
        }

        public int AverageUploadBandwith
        {
            get
            {
                return _uploadBandwith.Count > 0 ? UploadBandwith / _uploadBandwith.Count : -1;
            }
        }
    }
}

