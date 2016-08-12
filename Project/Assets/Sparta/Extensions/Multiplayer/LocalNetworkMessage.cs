﻿
using SocialPoint.IO;
using System.IO;

namespace SocialPoint.Multiplayer
{
    public class LocalNetworkMessage : INetworkMessage
    {
        public IWriter Writer{ get; private set; }
        public NetworkMessageData Data{ get; private set; }

        LocalNetworkServer _server;
        LocalNetworkClient[] _clients;
        LocalNetworkClient _origin;
        MemoryStream _stream;

        public LocalNetworkMessage(NetworkMessageData data, LocalNetworkClient[] clients)
        {
            _clients = clients;
            Init(data);
        }

        public LocalNetworkMessage(NetworkMessageData data, LocalNetworkClient origin, LocalNetworkServer server)
        {
            _origin = origin;
            _server = server;
            Init(data);
        }

        void Init(NetworkMessageData data)
        {
            Data = data;
            _stream = new MemoryStream();
            Writer = new SystemBinaryWriter(_stream);
        }

        public void Send()
        {
            Writer = null;
            if(_server != null)
            {
                _server.OnLocalMessageReceived(_origin, this);
            }
            if(_clients != null)
            {
                for(var i = 0; i < _clients.Length; i++)
                {
                    var client = _clients[i];
                    if(client != null)
                    {
                        client.OnLocalMessageReceived(this);
                    }
                }
            }
        }

        public IReader Receive()
        {
            return new SystemBinaryReader(new MemoryStream(_stream.GetBuffer()));
        }
    }
}