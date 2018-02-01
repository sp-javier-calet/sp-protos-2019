using System;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Network
{
    public sealed class LocalNetworkClient : INetworkClient
    {
        INetworkMessageReceiver _receiver;
        List<INetworkClientDelegate> _delegates = new List<INetworkClientDelegate>();
        ILocalNetworkServer _server;

        public byte ClientId{ get; private set; }

        public bool Connected{ get; private set; }

        public LocalNetworkClient(ILocalNetworkServer server)
        {
            _server = server;
        }

        public void Connect()
        {
            if(Connected)
            {
                return;
            }
            if(_server.Running)
            {
                Connected = true;
            }
            ClientId = _server.OnClientConnecting(this);
            if(_server.Running)
            {
                for(var i = 0; i < _delegates.Count; i++)
                {
                    _delegates[i].OnClientConnected();
                }
                _server.OnClientConnected(this);
            }
        }

        public void Disconnect()
        {
            if(!Connected)
            {
                return;
            }
            ClientId = 0;
            Connected = false;
            _server.OnClientDisconnected(this);
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientDisconnected();
            }
        }

        public void OnLocalMessageReceived(ILocalNetworkMessage msg)
        {
            if(!Connected)
            {
                return;
            }
            if(_receiver != null)
            {
                _receiver.OnMessageReceived(msg.Data, msg.Receive());
            }
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnMessageReceived(msg.Data);
            }
        }

        public void OnServerStarted()
        {
            if(!Connected)
            {
                Connect();
            }
        }

        public void OnServerStopped()
        {
            if(Connected)
            {
                Disconnect();
            }
        }

        public void OnServerFailed(Error err)
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnNetworkError(err);
            }
            Disconnect();
        }

        public INetworkMessage CreateMessage(NetworkMessageData info)
        {
            return new LocalNetworkMessage(info, this, Connected ? _server : null);
        }

        public void AddDelegate(INetworkClientDelegate dlg)
        {
            _delegates.Add(dlg);
            if(Connected && dlg != null)
            {
                dlg.OnClientConnected();
            }
        }

        public void RemoveDelegate(INetworkClientDelegate dlg)
        {
            _delegates.Remove(dlg);
        }

        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        public int GetDelay(int serverTimestamp)
        {
            return 0;
        }

        public bool LatencySupported
        {
            get
            {
                return false;
            }
        }

        public int Latency
        {
            get
            {
                DebugUtils.Assert(LatencySupported);
                return -1;
            }
        }
    }

    public class LocalNetworkClientFactory : INetworkClientFactory
    {
        readonly INetworkServerFactory _serverFactory;

        public LocalNetworkClientFactory(INetworkServerFactory serverFactory)
        {
            _serverFactory = serverFactory;
        }

        #region INetworkClientFactory implementation

        INetworkClient INetworkClientFactory.Create()
        {
            return new LocalNetworkClient(_serverFactory.Create() as LocalNetworkServer);
        }

        #endregion
    }
}
