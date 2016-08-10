
using SocialPoint.IO;
using System.IO;

namespace SocialPoint.Multiplayer
{
    public class LocalNetworkMessage : INetworkMessage
    {
        public IWriter Writer{ get; private set; }
        byte _type;
        int _channelId;

        LocalNetworkServer _server;
        LocalNetworkClient[] _clients;
        LocalNetworkClient _origin;
        MemoryStream _stream;

        public LocalNetworkMessage(NetworkMessageInfo info, LocalNetworkClient[] clients)
        {
            _clients = clients;
            Init(info);
        }

        public LocalNetworkMessage(NetworkMessageInfo info, LocalNetworkClient origin, LocalNetworkServer server)
        {
            _origin = origin;
            _server = server;
            Init(info);
        }

        void Init(NetworkMessageInfo info)
        {
            _type = info.MessageType;
            _channelId = info.ChannelId;
            _stream = new MemoryStream();
            Writer = new SystemBinaryWriter(_stream);
        }

        public void Send()
        {
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

        public ReceivedNetworkMessage Receive()
        {
            return new ReceivedNetworkMessage(_type, _channelId, new SystemBinaryReader(_stream));
        }
    }
}