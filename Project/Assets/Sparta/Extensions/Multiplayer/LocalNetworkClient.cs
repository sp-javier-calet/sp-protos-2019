using SocialPoint.Base;
using System.Collections.Generic;

namespace SocialPoint.Multiplayer
{

    public class LocalNetworkClient : INetworkClient
    {
        List<INetworkClientDelegate> _delegates = new List<INetworkClientDelegate>();
        LocalNetworkServer _server;

        public LocalNetworkClient(LocalNetworkServer server)
        {
            _server = server;
        }

        public void Connect()
        {
            _server.OnClientConnected(this);
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnConnected();
            }
        }

        public void Disconnect()
        {
            _server.OnClientDisconnected(this);
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnDisconnected();
            }
        }

        public void OnLocalMessageReceived(LocalNetworkMessage msg)
        {
            var received = msg.Receive();
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnMessageReceived(received);
            }
        }

        public INetworkMessage CreateMessage(byte type, int channelId)
        {
            return new LocalNetworkMessage(type, channelId, this, _server);
        }

        public void AddDelegate(INetworkClientDelegate dlg)
        {
            _delegates.Add(dlg);
        }

        public void RemoveDelegate(INetworkClientDelegate dlg)
        {
            _delegates.Remove(dlg);
        }
    }
}
