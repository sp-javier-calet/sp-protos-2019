using SocialPoint.Base;
using System.Collections.Generic;
using System;

namespace SocialPoint.Multiplayer
{
    public class LocalNetworkServer : INetworkServer
    {
        List<INetworkServerDelegate> _delegates = new List<INetworkServerDelegate>();
        Dictionary<LocalNetworkClient,byte> _clients = new Dictionary<LocalNetworkClient,byte>();

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void OnClientConnected(LocalNetworkClient client)
        {
            byte clientId = 0;
            bool found = false;
            for(; clientId < byte.MaxValue; clientId++)
            {
                if(!_clients.ContainsValue(clientId))
                {
                    found = true;
                    break;
                }
            }
            if(!found)
            {
                throw new InvalidOperationException("Too many clients.");
            }
            _clients[client] = clientId;
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientConnected(clientId);
            }
        }

        public void OnClientDisconnected(LocalNetworkClient client)
        {
            byte clientId;
            if(!_clients.TryGetValue(client, out clientId))
            {
                return;
            }
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientDisconnected(clientId);
            }
        }

        public void OnLocalMessageReceived(LocalNetworkClient origin, LocalNetworkMessage msg)
        {
            byte clientId;
            if(!_clients.TryGetValue(origin, out clientId))
            {
                return;
            }
            var received = msg.Receive();
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnMessageReceived(clientId, received);
            }
        }

        public INetworkMessage CreateMessage(byte type, int channelId)
        {
            var clients = new LocalNetworkClient[_clients.Count];
            _clients.Keys.CopyTo(clients, 0);
            return new LocalNetworkMessage(type, channelId, clients);
        }

        public INetworkMessage CreateMessage(byte clientId, byte type, int channelId)
        {
            var itr = _clients.GetEnumerator();
            LocalNetworkClient receiver = null;
            while(itr.MoveNext())
            {
                if(itr.Current.Value == clientId)
                {
                    receiver = itr.Current.Key;
                    break;
                }
            }
            itr.Dispose();
            if(receiver == null)
            {
                throw new InvalidOperationException("Could not find client id.");
            }
            return new LocalNetworkMessage(type, channelId, new LocalNetworkClient[]{ receiver });
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
