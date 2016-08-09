using SocialPoint.Base;
using System.Collections.Generic;

namespace SocialPoint.Multiplayer
{
    public class LocalNetworkServer : INetworkServer, ILocalNetworkMessageReceiver
    {
        List<INetworkServerDelegate> _delegates = new List<INetworkServerDelegate>();
        LocalNetworkClient _client;

        public void Start()
        {
        }

        public void Stop()
        {
        }

        const byte DefaultClientId = 0;

        public void OnClientConnected(LocalNetworkClient client)
        {
            _client = client;
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientConnected(DefaultClientId);
            }
        }

        public void OnClientDisconnected(LocalNetworkClient client)
        {
            _client = null;
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientDisconnected(DefaultClientId);
            }
        }

        public void OnLocalMessageReceived(LocalNetworkMessage msg)
        {
            var received = msg.Receive();
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnMessageReceived(DefaultClientId, received);
            }
        }

        public INetworkMessage CreateMessage(byte type, int channelId)
        {
            return new LocalNetworkMessage(type, channelId, _client);
        }

        public INetworkMessage CreateMessage(byte clientId, byte type, int channelId)
        {
            return CreateMessage(type, channelId);
        }

        public void AddDelegate(INetworkServerDelegate dlg)
        {
            _delegates.Add(dlg);
        }

        public void RemoveDelegate(INetworkServerDelegate dlg)
        {
            _delegates.Remove(dlg);
        }
    }
}
