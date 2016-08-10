using SocialPoint.Base;
using System.Collections.Generic;
using System;

namespace SocialPoint.Multiplayer
{

    public class LocalNetworkClient : INetworkClient
    {
        List<INetworkClientDelegate> _delegates = new List<INetworkClientDelegate>();
        LocalNetworkServer _server;

        public bool Connected;

        public LocalNetworkClient(LocalNetworkServer server)
        {
            _server = server;
        }

        public void Connect()
        {
            _server.OnClientConnecting(this);
            if(_server.Running)
            {
                Connected = true;
                for(var i = 0; i < _delegates.Count; i++)
                {
                    _delegates[i].OnConnected();
                }
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

        public INetworkMessage CreateMessage(NetworkMessageInfo info)
        {
            if(!Connected)
            {
                throw new InvalidOperationException("Client not connected.");
            }
            return new LocalNetworkMessage(info, this, _server);
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
