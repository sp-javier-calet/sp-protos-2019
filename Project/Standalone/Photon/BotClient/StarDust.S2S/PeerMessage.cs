using SocialPoint.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPoint.IO;
using Photon.SocketServer.ServerToServer;
using System.IO;

namespace Photon.Stardust.S2S.Server
{
    class PeerMessage : INetworkMessage
    {
        S2SPeerBase _peer;
        NetworkMessageData _data;
        IWriter _writer;
        MemoryStream _stream;

        const byte EventDataKey = 245;
        const byte SenderActorKey = 254;

        Action<byte, byte[]> _doSend;

        public PeerMessage(S2SPeerBase peer, NetworkMessageData data, Action<byte, byte[]> sendDelegate)
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
