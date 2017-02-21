using SocialPoint.Network;
using System;
using SocialPoint.IO;
using Photon.Stardust.S2S.Server;
using Photon.SocketServer.ServerToServer;
using System.IO;

namespace SocialPoint.Network
{
    class StardustNetworkMessage : INetworkMessage
    {
        S2SPeerBase _peer;
        NetworkMessageData _data;
        IWriter _writer;
        MemoryStream _stream;

        const byte EventDataKey = 245;
        const byte SenderActorKey = 254;

        Action<byte, byte[]> _doSend;

        public StardustNetworkMessage(S2SPeerBase peer, NetworkMessageData data, Action<byte, byte[]> sendDelegate)
        {
            _peer = peer;
            _data = data;
            _stream = new MemoryStream();
            _writer = new SystemBinaryWriter(_stream);
            _doSend = sendDelegate;
        }

        public IWriter Writer
        {
            get
            {
                return _writer;
            }
        }

        public void Send()
        {
            _doSend(_data.MessageType, _stream.ToArray());
        }
    }
}
