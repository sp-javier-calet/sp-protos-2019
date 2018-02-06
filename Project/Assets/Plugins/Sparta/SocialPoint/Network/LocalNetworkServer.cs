using System;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public interface ILocalNetworkServer : INetworkServer
    {
        byte OnClientConnecting(LocalNetworkClient client);
        void OnClientConnected(LocalNetworkClient client);
        void OnClientDisconnected(LocalNetworkClient client);
        void OnLocalMessageReceived(LocalNetworkClient origin, ILocalNetworkMessage msg);
        ILocalNetworkMessage CreateLocalMessage(NetworkMessageData data);
    }

    public sealed class LocalNetworkServer : ILocalNetworkServer
    {
        List<INetworkServerDelegate> _delegates = new List<INetworkServerDelegate>();
        Dictionary<LocalNetworkClient, byte> _clients = new Dictionary<LocalNetworkClient,byte>();
        List<LocalNetworkClient> _clientList = new List<LocalNetworkClient>();
        INetworkMessageReceiver _receiver;

        public bool Running{ get; private set; }

        public string Id{ get; set; }

        public LocalNetworkServer()
        {
            Id = RandomUtils.GetUuid();
        }

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
            _clientList.Clear();
            _clientList.AddRange(_clients.Keys);
            for(var i = 0; i < _clientList.Count; i++)
            {
                _clientList[i].OnServerStarted();
            }
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
            _clientList.Clear();
            _clientList.AddRange(_clients.Keys);
            for(var i = 0; i < _clientList.Count; i++)
            {
                _clientList[i].OnServerStopped();
            }
        }

        public void Fail(Error err)
        {
            _clientList.Clear();
            _clientList.AddRange(_clients.Keys);
            for(var i = 0; i < _clientList.Count; i++)
            {
                _clientList[i].OnServerFailed(err);
            }
            Stop();
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
            return clientId;
        }

        public void OnClientConnected(LocalNetworkClient client)
        {
            byte clientId;
            if(Running && _clients.TryGetValue(client, out clientId))
            {
                for(var i = 0; i < _delegates.Count; i++)
                {
                    _delegates[i].OnClientConnected(clientId);
                }
            }
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

        public void OnLocalMessageReceived(LocalNetworkClient origin, ILocalNetworkMessage msg)
        {
            byte clientId;
            if(!_clients.TryGetValue(origin, out clientId))
            {
                return;
            }
            NetworkMessageData data = msg.Data;
            data.ClientIds = new List<byte>(){ clientId };
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
            return CreateLocalMessage(info);
        }

        public ILocalNetworkMessage CreateLocalMessage(NetworkMessageData info)
        {
            if(!Running)
            {
                return new LocalNetworkMessage(info, null);
            }
            LocalNetworkClient[] clients;
            if(info.ClientIds != null && info.ClientIds.Count > 0)
            {
                var itr = _clients.GetEnumerator();
                LocalNetworkClient client = null;
                while(itr.MoveNext())
                {
                    if(itr.Current.Value == info.ClientIds[0])
                    {
                        client = itr.Current.Key;
                        break;
                    }
                }
                itr.Dispose();
                if(client == null)
                {
                    clients = new LocalNetworkClient[]{ };
                }
                else
                {
                    clients = new LocalNetworkClient[]{ client };
                }
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

        public bool LatencySupported
        {
            get
            {
                return false;
            }
        }

        void IDisposable.Dispose()
        {
            Stop();
            _delegates.Clear();
            _clients.Clear();
            _clientList.Clear();
        }
    }

    public interface ILocalNetworkServerFactory : INetworkServerFactory
    {
        ILocalNetworkServer Server { get; }
    }

    public class LocalNetworkServerFactory : ILocalNetworkServerFactory
    {
        #region INetworkServerFactory implementation

        public ILocalNetworkServer Server { get; private set; }

        INetworkServer INetworkServerFactory.Create()
        {
            Server = new LocalNetworkServer();
            return Server;
        }

        #endregion
    }
}