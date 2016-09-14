using SocialPoint.Base;
using SocialPoint.Utils;
using System.Collections.Generic;
using System;

namespace SocialPoint.Network
{
    public sealed class LocalNetworkServer : INetworkServer
    {
        List<INetworkServerDelegate> _delegates = new List<INetworkServerDelegate>();
        Dictionary<LocalNetworkClient, byte> _clients = new Dictionary<LocalNetworkClient,byte>();
        INetworkMessageReceiver _receiver;

        public bool Running{ get; private set; }

        public void Start()
        {
            if(Running)
            {
                return;
            }
            Running = true;
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnServerStarted();
            }
            var clients = new List<LocalNetworkClient>(_clients.Keys);
            var itr = clients.GetEnumerator();
            while(itr.MoveNext())
            {
                itr.Current.OnServerStarted();
            }
            itr.Dispose();
        }

        public void Stop()
        {
            if(!Running)
            {
                return;
            }
            Running = false;
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnServerStopped();
            }
            var clients = new List<LocalNetworkClient>(_clients.Keys);
            var itr = clients.GetEnumerator();
            while(itr.MoveNext())
            {
                itr.Current.OnServerStopped();
            }
            itr.Dispose();
        }

        public byte OnClientConnecting(LocalNetworkClient client)
        {
            byte clientId = 1;
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

            if(Running)
            {
                for(var i = 0; i < _delegates.Count; i++)
                {
                    _delegates[i].OnClientConnected(clientId);
                }
            }

            return clientId;
        }

        public void OnClientDisconnected(LocalNetworkClient client)
        {
            byte clientId;
            if(!_clients.TryGetValue(client, out clientId))
            {
                return;
            }
            _clients.Remove(client);
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
            NetworkMessageData data = msg.Data;
            data.ClientId = clientId;
            if(_receiver != null)
            {
                _receiver.OnMessageReceived(data, msg.Receive());
            }
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnMessageReceived(data);
            }
        }

        public INetworkMessage CreateMessage(NetworkMessageData info)
        {
            if(!Running)
            {
                throw new InvalidOperationException("Server not running.");
            }
            LocalNetworkClient[] clients;
            if(info.ClientId > 0)
            {
                var itr = _clients.GetEnumerator();
                LocalNetworkClient client = null;
                while(itr.MoveNext())
                {
                    if(itr.Current.Value == info.ClientId)
                    {
                        client = itr.Current.Key;
                        break;
                    }
                }
                itr.Dispose();
                if(client == null)
                {
                    throw new InvalidOperationException("Could not find client id.");
                }
                clients = new LocalNetworkClient[]{ client };
            }
            else
            {
                clients = new LocalNetworkClient[_clients.Count];
                _clients.Keys.CopyTo(clients, 0);

            }
            return new LocalNetworkMessage(info, clients);
        }

        public void AddDelegate(INetworkServerDelegate dlg)
        {
            _delegates.Add(dlg);
            if(Running && dlg != null)
            {
                dlg.OnServerStarted();
            }
        }

        public void RemoveDelegate(INetworkServerDelegate dlg)
        {
            _delegates.Remove(dlg);
        }

        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            _receiver = receiver;
        }
        public int GetTimestamp()
        {
            return (int)TimeUtils.TimestampMilliseconds;
        }
    }
}
