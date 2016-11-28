using SocialPoint.Utils;
using SocialPoint.Network;
using SocialPoint.Matchmaking;
using SocialPoint.IO;
using SocialPoint.Lockstep;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Photon.Hive.Plugin.Lockstep
{
    /**
     * more info:
     * https://doc.photonengine.com/en/onpremise/current/plugins/manual
     * https://doc.photonengine.com/en/onpremise/current/plugins/plugins-faq
     * https://doc.photonengine.com/en/onpremise/current/plugins/plugins-upload-guide
     */
    public class LockstepPlugin : PluginBase, INetworkServer
    {
        public override string Name
        {
            get
            {
                return "Lockstep";
            }
        }

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

        LockstepNetworkServer _netServer;
        List<INetworkServerDelegate> _delegates;
        INetworkMessageReceiver _receiver;
        HttpMatchmakingServer _matchmaking;
        Examples.Lockstep.ServerBehaviour _game;
        object _timer;

        const byte MaxPlayersKey = 255;
        const byte MasterClientIdKey = 248;
        const byte IsOpenKey = 253;
        const byte ErrorInfoCode = 251;
        const byte ParameterCodeData = 245;
        const int NoRandomMatchFoundCode = 32760;
        const string ServerIdRoomProperty = "server";

        public LockstepPlugin()
        {
            UseStrictMode = true;
            _delegates = new List<INetworkServerDelegate>();
            _matchmaking = new HttpMatchmakingServer(
                new WebRequestHttpClient(new ImmediateCoroutineRunner()));
            _netServer = new LockstepNetworkServer(this, _matchmaking);
            _game = new Examples.Lockstep.ServerBehaviour(_netServer);
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
            PluginHost.StopTimer(_timer);
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
                { (int)MaxPlayersKey, (int)_netServer.MaxPlayers },
                { (int)MasterClientIdKey, 0 },
                { ServerIdRoomProperty, 0 },
            }, null, false);
            for (var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnServerStarted();
            }

            var clientId = GetClientId(info.UserId);
            OnClientConnected(clientId);
        }

        void OnClientConnected(byte clientId)
        {
            for (var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientConnected(clientId);
            }
            UpdateRoomOpen();
        }

        void UpdateRoomOpen()
        {
            if (_netServer != null)
            {
                PluginHost.SetProperties(0,
                    new Hashtable { { (int)IsOpenKey, !_netServer.Full } }, null, false);
            }
        }

        bool CheckServer(ICallInfo info)
        {
            if (_netServer.Full)
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

        void OnClientDisconnected(byte clientId)
        {
            for (var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientDisconnected(clientId);
            }
            UpdateRoomOpen();
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

        const string CommandStepDurationConfig = "CommandStepDuration";
        const string SimulationStepDurationConfig = "SimulationStepDuration";
        const string MaxPlayersConfig = "MaxPlayers";
        const string ClientStartDelayConfig = "ClientStartDelay";
        const string ClientSimulationDelayConfig = "ClientSimulationDelay";
        const string BackendBaseUrlConfig = "BackendBaseUrl";

        int GetConfigOption(Dictionary<string, string> config, string key, int def)
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

        public override bool SetupInstance(IPluginHost host, Dictionary<string, string> config, out string errorMsg)
        {
            if (!base.SetupInstance(host, config, out errorMsg))
            {
                return false;
            }
            _netServer.Config.CommandStepDuration = GetConfigOption(config,
                CommandStepDurationConfig, _netServer.Config.CommandStepDuration);
            _netServer.Config.SimulationStepDuration = GetConfigOption(config,
                SimulationStepDurationConfig, _netServer.Config.SimulationStepDuration);
            _netServer.ServerConfig.MaxPlayers = (byte)GetConfigOption(config,
                MaxPlayersConfig, _netServer.ServerConfig.MaxPlayers);
            _netServer.ServerConfig.ClientStartDelay = GetConfigOption(config,
                ClientStartDelayConfig, _netServer.ServerConfig.ClientStartDelay);
            _netServer.ServerConfig.ClientSimulationDelay = GetConfigOption(config,
                ClientSimulationDelayConfig, _netServer.ServerConfig.ClientSimulationDelay);

            string baseUrl;
            if (_matchmaking != null && config.TryGetValue(BackendBaseUrlConfig, out baseUrl))
            {
                _matchmaking.BaseUrl = baseUrl;
            }
            _timer = PluginHost.CreateTimer(Update, 0, _netServer.Config.CommandStepDuration);
            return true;
        }

        void Update()
        {
            try
            {
                _netServer.Update();
            }
            catch(Exception e)
            {
                HandleException(e);
            }
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
