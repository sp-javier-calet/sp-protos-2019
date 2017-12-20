using System;
using System.Collections.Generic;
using SocialPoint.IO;
using SocialPoint.Network;

namespace SocialPoint.Network
{
    public interface INetworkMatchDelegateFactory
    {
        object Create(string matchId, INetworkMessageSender sender);
    }

    public sealed class MatchConnectMessage : INetworkShareable
    {

        public string MatchId { get; private set; }

        public MatchConnectMessage(string matchId = null)
        {
            MatchId = matchId;
        }

        public void Deserialize(IReader reader)
        {
            MatchId = reader.ReadString();
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(MatchId);
        }
    }

    public class MultiMatchController : INetworkServerDelegate, INetworkMessageReceiver, IDisposable
    {
        class MatchInfo
        {
            public string Id;
            public List<byte> ClientIds = new List<byte>();
            public INetworkServerDelegate Delegate;
            public INetworkMessageReceiver Receiver;
        }


        class MatchNetworkSender : INetworkMessageSender
        {
            INetworkServer _server;
            MatchInfo _info;

            public MatchNetworkSender(INetworkServer server, MatchInfo info)
            {
                _server = server;
                _info = info;
            }

            public INetworkMessage CreateMessage(NetworkMessageData data)
            {
                if(data.ClientIds == null)
                {
                    data.ClientIds = new List<byte>();
                }
                if(data.ClientIds.Count == 0)
                {
                    data.ClientIds.AddRange(_info.ClientIds);
                }
                return _server.CreateMessage(data);
            }
        }

        byte _connectMessageType;

        INetworkServer _netServer;

        INetworkMatchDelegateFactory _factory;

        List<MatchInfo> _matches;

        public MultiMatchController(INetworkServer server, INetworkMatchDelegateFactory factory, byte connectMessageType)
        {
            _netServer = server;
            _factory = factory;
            _connectMessageType = connectMessageType;
            _matches = new List<MatchInfo>();
            _netServer.AddDelegate(this);
            _netServer.RegisterReceiver(this);
        }

        #region MATCH INFO

        MatchInfo CreateOrJoinMatch(byte clientId, string matchId)
        {
            var match = GetMatch(matchId);
            if(match == null)
            {
                match = new MatchInfo();
                match.Id = matchId;
                var obj = _factory.Create(matchId, new MatchNetworkSender(_netServer, match)); 
                match.Delegate = obj as INetworkServerDelegate;
                match.Delegate.OnServerStarted();
                match.Receiver = obj as INetworkMessageReceiver;
                _matches.Add(match);
            }
            match.ClientIds.Add(clientId);
            if(match.Delegate != null)
            {
                match.Delegate.OnClientConnected(clientId);
            }
            return match;
        }

        MatchInfo GetMatch(byte clientId)
        {
            return _matches.Find(m => m.ClientIds.Contains(clientId));
        }


        MatchInfo GetMatch(string matchId)
        {
            return _matches.Find(m => m.Id == matchId);
        }

        #endregion

        #region INetworkServerDelegate implementation

        void INetworkServerDelegate.OnServerStarted()
        {
        }

        void INetworkServerDelegate.OnServerStopped()
        {
            for(int i = 0; i < _matches.Count; i++)
            {
                var match = _matches[i];
                if(match != null && match.Delegate != null)
                {
                    match.Delegate.OnServerStopped();
                }
            }
            _matches.Clear();
        }

        void INetworkServerDelegate.OnClientConnected(byte clientId)
        {
        }

        void INetworkServerDelegate.OnClientDisconnected(byte clientId)
        {
            var match = GetMatch(clientId);
            if(match != null && match.Delegate != null)
            {
                match.Delegate.OnClientDisconnected(clientId);
                match.ClientIds.Remove(clientId);

                if(match.ClientIds.Count == 0)
                {
                    match.Delegate.OnServerStopped();
                    _matches.Remove(match);
                    match.Delegate = null;
                    match.Receiver = null;
                }

            }
        }

        void INetworkServerDelegate.OnMessageReceived(NetworkMessageData data)
        {
            var match = GetMatch(data.ClientIds[0]);
            if(match != null)
            {
                if(match.Delegate != null)
                {
                    match.Delegate.OnMessageReceived(data);
                }
            }
        }

        void INetworkServerDelegate.OnNetworkError(SocialPoint.Base.Error err)
        {
            for(var i = 0; i < _matches.Count; i++)
            {
                var dlg = _matches[i].Delegate;
                if(dlg != null)
                {
                    dlg.OnNetworkError(err);
                }
            }
        }

        #endregion

        #region INetworkMessageReceiver implementation

        void INetworkMessageReceiver.OnMessageReceived(NetworkMessageData data, SocialPoint.IO.IReader reader)
        {
            if(data.MessageType == _connectMessageType)
            {
                var cmd = new MatchConnectMessage();
                cmd.Deserialize(reader);
                CreateOrJoinMatch(data.ClientIds[0], cmd.MatchId);
            }
            else
            {
                var match = GetMatch(data.ClientIds[0]);
                if(match != null)
                {
                    if(match.Delegate != null)
                    {
                        match.Delegate.OnMessageReceived(data);
                    }

                    if(match.Receiver != null)
                    {
                        match.Receiver.OnMessageReceived(data, reader);
                    }
                }
            }
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            _netServer.RemoveDelegate(this);
            _netServer.RegisterReceiver(null);
            ((INetworkServerDelegate)this).OnServerStopped();
        }

        #endregion
    }
}
