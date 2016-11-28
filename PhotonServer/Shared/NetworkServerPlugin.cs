using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Photon.Hive.Plugin;
using SocialPoint.IO;

namespace SocialPoint.Network
{
    /**
     * more info:
     * https://doc.photonengine.com/en/onpremise/current/plugins/manual
     * https://doc.photonengine.com/en/onpremise/current/plugins/plugins-faq
     * https://doc.photonengine.com/en/onpremise/current/plugins/plugins-upload-guide
     */
    public abstract class NetworkServerPlugin : PluginBase, INetworkServer
    {

        bool INetworkServer.Running
        {
            get
            {
                return true;
            }
        }

        string INetworkServer.Id
        {
            get
            {
                return PluginHost.GameId;
            }
        }

        List<INetworkServerDelegate> _delegates;
        INetworkMessageReceiver _receiver;
        object _timer;

        const byte ErrorInfoCode = 251;
        const byte ParameterCodeData = 245;
        const byte MaxPlayersKey = 255;
        const byte IsOpenKey = 253;
        const string ServerIdRoomProperty = "server";
        const byte MasterClientIdKey = 248;

        abstract protected int MaxPlayers { get; }
        abstract protected bool Full { get; }
        abstract protected int UpdateInterval { get;  }

        public NetworkServerPlugin()
        {
            UseStrictMode = true;
            _delegates = new List<INetworkServerDelegate>();
        }

        bool CheckServer(ICallInfo info)
        {
            if (Full)
            {
                info.Fail("Game is full.");
            }
            else
            {
                info.Continue();
                return true;
            }
            return false;
        }

        byte GetClientId(string userId)
        {
            var actors = PluginHost.GameActors;
            for(var i=0; i<actors.Count; i++)
            {
                var actor = actors[i];
                if (actor.UserId == userId)
                {
                    return GetClientId(actor.ActorNr);
                }
            }
            return 0;
        }

        byte GetClientId(int actorId)
        {
            return (byte)actorId;
        }

        public override void OnCloseGame(ICloseGameCallInfo info)
        {
            if (_timer != null)
            {
                PluginHost.StopTimer(_timer);
            }
            for (var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnServerStopped();
            }
            info.Continue();
        }

        public override void OnCreateGame(ICreateGameCallInfo info)
        {
            if (!CheckServer(info))
            {
                return;
            }

            PluginHost.SetProperties(0, new Hashtable {
                { (int)MaxPlayersKey,MaxPlayers },
                { (int)MasterClientIdKey, 0 },
                { ServerIdRoomProperty, 0 },
            }, null, false);

            for (var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnServerStarted();
            }

            var u = UpdateInterval;
            if (u > 0)
            {
                _timer = PluginHost.CreateTimer(TryUpdate, 0, u);
            }
            var clientId = GetClientId(info.UserId);
            OnClientConnected(clientId);
        }

        protected virtual void OnClientConnected(byte clientId)
        {
            for (var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientConnected(clientId);
            }
            OnClientChanged();
        }

        public override void BeforeJoin(IBeforeJoinGameCallInfo info)
        {
            CheckServer(info);
        }

        public override void OnJoin(IJoinGameCallInfo info)
        {
            info.Continue();
            OnClientConnected(GetClientId(info.ActorNr));
        }

        public override void OnLeave(ILeaveGameCallInfo info)
        {
            OnClientDisconnected(GetClientId(info.ActorNr));
            info.Continue();
        }

        protected virtual void OnClientDisconnected(byte clientId)
        {
            for (var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientDisconnected(clientId);
            }
            OnClientChanged();
        }

        protected virtual void OnClientChanged()
        {
            PluginHost.SetProperties(0,
                new Hashtable { { (int)IsOpenKey, !Full } }, null, false);
        }

        public override void OnSetProperties(ISetPropertiesCallInfo info)
        {
            if (info.Request.Properties.ContainsKey(ServerIdRoomProperty))
            {
                info.Fail("This room already has a server.");
            }
            else
            {
                info.Continue();
            }
        }

        public override void OnRaiseEvent(IRaiseEventCallInfo info)
        {
            info.Continue();
            if (_receiver == null)
            {
                return;
            }
            try
            {
                var data = info.Request.Data as byte[];
                if (data != null)
                {
                    var stream = new MemoryStream(data);
                    var reader = new SystemBinaryReader(stream);
                    var netData = new NetworkMessageData
                    {
                        ClientId = GetClientId(info.ActorNr),
                        MessageType = info.Request.EvCode
                    };
                    _receiver.OnMessageReceived(netData, reader);
                }
            }
            catch(Exception e)
            {
                HandleException(e);
            }
        }

        protected static int GetConfigOption(Dictionary<string, string> config, string key, int def)
        {
            string sval;
            if(config.TryGetValue(key, out sval))
            {
                int val;
                if(int.TryParse(sval, out val))
                {
                    return val;
                }
            }
            return def;
        }

        void TryUpdate()
        {
            try
            {
                Update();
            }
            catch(Exception e)
            {
                HandleException(e);
            }
        }

        protected virtual void Update()
        {
        }

        void BroadcastError(string message)
        {
            var errorMsg = "[Server Error]: " + message;
            var dic = new Dictionary<byte, object>();
            dic.Add(ParameterCodeData, errorMsg);
            BroadcastEvent(ErrorInfoCode, dic);
            PluginHost.LogError(errorMsg);
        }

        void HandleException(Exception e)
        {
            BroadcastError(e.Message);
        }

        void INetworkServer.Start()
        {
            for (var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnServerStarted();
            }
        }

        void INetworkServer.Stop()
        {
            for (var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnServerStopped();
            }
            BroadcastError("server stopped");
        }

        void INetworkServer.Fail(string reason)
        {
            BroadcastError(reason);
        }

        INetworkMessage INetworkMessageSender.CreateMessage(NetworkMessageData info)
        {
            List<int> actors = null;
            if(info.ClientId != 0)
            {
                actors = new List<int>();
                actors.Add(info.ClientId);
            }
            return new PluginNetworkMessage(PluginHost, info.MessageType, info.Unreliable, actors);
        }

        void INetworkServer.AddDelegate(INetworkServerDelegate dlg)
        {
            _delegates.Add(dlg);
        }

        void INetworkServer.RemoveDelegate(INetworkServerDelegate dlg)
        {
            _delegates.Remove(dlg);
        }

        void INetworkServer.RegisterReceiver(INetworkMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        int INetworkServer.GetTimestamp()
        {
            return System.Environment.TickCount;
        }
    }
}
