using SocialPoint.IO;
using System;
using System.IO;

namespace SocialPoint.Network
{
    class PhotonNetworkMessage : INetworkMessage
    {
        NetworkMessageData _data;
        SystemBinaryWriter _writer;
        MemoryStream _stream;
        PhotonNetworkBase _sender;

        public PhotonNetworkMessage(NetworkMessageData data, PhotonNetworkBase sender)
        {
            if(data.MessageType >= EventCode.LobbyStats)
            {
                throw new ArgumentException("Message type is too big.");
            }
            _data = data;
            _stream = new MemoryStream();
            _writer = new SystemBinaryWriter(_stream);
            _sender = sender;
        }

        public void Send()
        {
            _sender.SendNetworkMessage(_data, _stream.ToArray());
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
