
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

        public LocalNetworkMessage(byte type, int chanId, LocalNetworkClient[] clients)
        {
            _clients = clients;
            Init(type, chanId);
        }

        public LocalNetworkMessage(byte type, int chanId, LocalNetworkClient origin, LocalNetworkServer server)
        {
            _origin = origin;
            _server = server;
            Init(type, chanId);
        }

        void Init(byte type, int chanId)
        {
            _type = type;
            _channelId = chanId;
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
                    _clients[i].OnLocalMessageReceived(this);
                }
            }
        }

        public ReceivedNetworkMessage Receive()
        {
            return new ReceivedNetworkMessage(_type, _channelId, new SystemBinaryReader(_stream));
        }
    }
}