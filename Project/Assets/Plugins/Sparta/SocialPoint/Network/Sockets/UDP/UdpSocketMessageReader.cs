using System;
using System.Collections.Generic;
using System.IO;
using SocialPoint.IO;
using SocialPoint.Network;
using LiteNetLib.Utils;

namespace SocialPoint.Network
{
    public class UdpSocketMessageReader
    {
        MemoryStream _memStream;
        SystemBinaryReader _reader;
        byte Type;
        byte _clientId;


        public event Action<NetworkMessageData, IReader> MessageReceived;

        public byte ClientId
        {
            get
            {
                return _clientId;
            }
        }

        public UdpSocketMessageReader(byte clientId = 0)
        {
            _clientId = clientId;
            _memStream = new MemoryStream();
            _reader = new SystemBinaryReader(_memStream);
        }

        public void Receive(NetDataReader reader)
        {
            byte[] bytes = reader.Data;
            _memStream.Write(bytes, 0, bytes.Length);
            _memStream.Seek(0, SeekOrigin.Begin);
            Type = _reader.ReadByte();
            _memStream.Seek(1, SeekOrigin.Begin);
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
