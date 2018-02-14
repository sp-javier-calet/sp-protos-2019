
using System;
using System.Collections.Generic;
using System.IO;
using SocialPoint.IO;
using LiteNetLib;
using LiteNetLib.Utils;

namespace SocialPoint.Network
{
    public sealed class UdpSocketNetworkMessage : INetworkMessage
    {
        SystemBinaryWriter _writer;
        MemoryStream _memStream;
        NetDataWriter _netDataWriter;
        NetworkMessageData _data;
        List<NetPeer> _peers;

        public UdpSocketNetworkMessage(NetworkMessageData data, List<NetPeer> peers)
        {
            _data = data;
            _peers = peers;
            _netDataWriter = new NetDataWriter();
            _memStream = new MemoryStream();
            _writer = new SystemBinaryWriter(_memStream);
        }

        public void Send()
        {
            _netDataWriter.Reset();
            _memStream.Seek(0, SeekOrigin.Begin);
            var data = _memStream.ToArray();
            _netDataWriter.Put(_data.MessageType);
            _netDataWriter.Put(data, 0, data.Length);
            for(int i = 0; i < _peers.Count; i++)
            {
                _peers[i].Send(_netDataWriter, _data.Unreliable ? SendOptions.Unreliable : SendOptions.ReliableOrdered);
            }
        }

        public IWriter Writer
        {
            get
            {
                return _writer;
            }
        }
    }

}
