using System.Collections;
using System.Collections.Generic;
using SocialPoint.IO;
using System.IO;
using System;
using System.Net.Sockets;
using SocialPoint.Network;

namespace SocialPoint.Network
{
    public class TcpSocketMessageReader
    {
        byte Type;
        int Length;
        byte _clientId;
        TcpClient _client;
        NetworkStream _netStream;
        MemoryStream _memStream;
        SystemBinaryReader _reader;

        public event Action<NetworkMessageData, IReader> MessageReceived;

        public NetworkStream Stream
        {
            get
            {
                return _netStream;
            }
        }

        public byte ClientId
        {
            get
            {
                return _clientId;
            }
        }

        public TcpSocketMessageReader(NetworkStream netStream, byte clientId = 0)
        {
            _clientId = clientId;
            _netStream = netStream;
            _memStream = new MemoryStream();
            _reader = new SystemBinaryReader(_memStream);
        }

        public void Receive()
        {
            if(!_netStream.CanRead)
            {
                return;
            }

            byte[] bytes = new byte[1];
            _netStream.Read(bytes, 0, sizeof(byte));
            _memStream.Write(bytes, 0, sizeof(byte));

            if(_memStream.Position == sizeof(byte))
            {
                _memStream.Seek(0, SeekOrigin.Begin);
                Type = _reader.ReadByte();
            }
            if(_memStream.Position == sizeof(byte) + sizeof(Int32))
            {
                _memStream.Seek(1, SeekOrigin.Begin);
                Length = _reader.ReadInt32();
            }
            if(_memStream.Position == sizeof(byte) + sizeof(Int32) + Length)
            {
                _memStream.Seek(5, SeekOrigin.Begin);
                if(MessageReceived != null)
                {
                    MessageReceived(new NetworkMessageData {
                        MessageType = Type,
                        ClientIds = _clientId == 0 ? null : new List<byte>(){ _clientId },
                    }, _reader);
                }
                _memStream.Seek(0, SeekOrigin.Begin);
            }
        }
    }
}
