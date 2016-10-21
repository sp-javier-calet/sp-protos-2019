using System.Collections.Generic;
using SocialPoint.Utils;
using SocialPoint.Multiplayer;
using SocialPoint.Network;
using SocialPoint.IO;
using System.IO;
using System.Collections;

namespace Photon.Hive.Plugin.Authoritative
{
    /**
     * more info:
     * https://doc.photonengine.com/en/onpremise/current/plugins/manual
     * https://doc.photonengine.com/en/onpremise/current/plugins/plugins-faq
     * https://doc.photonengine.com/en/onpremise/current/plugins/plugins-upload-guide
     */
    public class AuthoritativePlugin : PluginBase, INetworkServer
    {
        public override string Name
        {
            get
            {
                return "Authoritative";
            }
        }

        bool INetworkServer.Running
        {
            get
            {
                return true;
            }
        }

        NetworkServerSceneController _netServer;
        List<INetworkServerDelegate> _delegates;
        INetworkMessageReceiver _receiver;
        object _timer;
 
        const byte MaxPlayersKey = 255;
        const byte MasterClientIdKey = 248;
        const byte IsOpenKey = 253;
        const int NoRandomMatchFoundCode = 32760;
        const string ServerIdRoomProperty = "server";

        //Game related variables
        GameMultiplayerServerBehaviour _gameServer;
        int _lastUpdateTimestamp = 0;
        int _updateIntervalMs = 100;
        string _navMeshFileLocation = "\\..\\data\\test_navmesh";

        public AuthoritativePlugin()
        {
            UseStrictMode = true;
            _delegates = new List<INetworkServerDelegate>();
            _netServer = new NetworkServerSceneController(this);
            _gameServer = new GameMultiplayerServerBehaviour(this, _netServer, new EmptyPhysicsDebugger());

            ((INetworkServerDelegate)_netServer).OnServerStarted();//TODO: Do here or inside INetworkServer.Start() implementation?

            string navmeshPath = typeof(AuthoritativePlugin).Assembly.Location + _navMeshFileLocation;
            string message = string.Empty;
            _gameServer.LoadNavMesh(navmeshPath, out message);
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
            ((INetworkServerDelegate)_netServer).OnServerStopped();//TODO: Do here or inside INetworkServer.Stop() implementation?
            info.Continue();
        }

        public override void OnCreateGame(ICreateGameCallInfo info)
        {
            if (!CheckServer(info))
            {
                return;
            }
            /*PluginHost.SetProperties(0, new Hashtable {
                { (int)MaxPlayersKey, _gameServer.MaxPlayers },
                { (int)MasterClientIdKey, 0 },
                { ServerIdRoomProperty, 0 },
            }, null, false);*/
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
                    new Hashtable { { (int)IsOpenKey, !_gameServer.Full } }, null, false);
            }
        }

        bool CheckServer(ICallInfo info)
        {
            if (_gameServer.Full)
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
            if (_receiver != null)
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
            /*_netServer.Config.CommandStepDuration = GetConfigOption(config,
                CommandStepDurationConfig, _netServer.Config.CommandStepDuration);
            _netServer.Config.SimulationStepDuration = GetConfigOption(config,
                SimulationStepDurationConfig, _netServer.Config.SimulationStepDuration);
            _netServer.ServerConfig.MaxPlayers = (byte)GetConfigOption(config,
                MaxPlayersConfig, _netServer.ServerConfig.MaxPlayers);
            _netServer.ServerConfig.ClientStartDelay = GetConfigOption(config,
                ClientStartDelayConfig, _netServer.ServerConfig.ClientStartDelay);
            _netServer.ServerConfig.ClientSimulationDelay = GetConfigOption(config,
                ClientSimulationDelayConfig, _netServer.ServerConfig.ClientSimulationDelay);*/

            _timer = PluginHost.CreateTimer(Update, 0, _updateIntervalMs);
            return true;
        }

        void Update()
        {
            int currentTimestamp = ((INetworkServer)this).GetTimestamp();
            float deltaTime = ((float)(currentTimestamp - _lastUpdateTimestamp)) * 0.001f;//Milliseconds to seconds
            _lastUpdateTimestamp = currentTimestamp;
            
            _netServer.Update(deltaTime);
        }

        void INetworkServer.Start()
        {   
        }

        void INetworkServer.Stop()
        { 
        }

        INetworkMessage INetworkServer.CreateMessage(NetworkMessageData info)
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
