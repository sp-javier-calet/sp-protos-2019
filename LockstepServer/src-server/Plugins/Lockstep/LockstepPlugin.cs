﻿using System.Collections.Generic;
using SocialPoint.Utils;
using SocialPoint.Network;
using SocialPoint.Lockstep;
using SocialPoint.Lockstep.Network;
using System;

namespace Photon.Hive.Plugin.Lockstep
{
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

        ServerLockstepNetworkController _netServer;
        UpdateScheduler _updateScheduler;
        List<INetworkServerDelegate> _delegates;
        INetworkMessageReceiver _receiver;
        LockstepCommandFactory _factory;
        object _timer;
        float _updateInterval;

        public LockstepPlugin()
        {
            _updateScheduler = new UpdateScheduler();
            _delegates = new List<INetworkServerDelegate>();
            _factory = new LockstepCommandFactory();
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
            _netServer.Stop();
            info.Continue();
        }

        public override void OnCreateGame(ICreateGameCallInfo info)
        {
            info.Continue();
            var clientId = GetClientId(info.UserId);
            OnClientConnected(clientId);
        }

        void OnClientConnected(byte clientId)
        {
            for (var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientConnected(clientId);
            }
        }

        public override void OnJoin(IJoinGameCallInfo info)
        {
            OnClientConnected(GetClientId(info.ActorNr));
            info.Continue();
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
        }

        public override void OnRaiseEvent(IRaiseEventCallInfo info)
        {
            info.Continue();
        }

        public override void OnSetProperties(ISetPropertiesCallInfo info)
        {
            info.Continue();
        }

        const string PlayersCountKey = "PlayersCount";
        const string StartDelayKey = "StartDelay";
        const string CommandStepFactorKey = "CommandStepFactor";
        const string SimulationStepKey = "SimulationStep";
        const string MinExecutionTurnAnticipationKey = "MinExecutionTurnAnticipation";
        const string MaxExecutionTurnAnticipationKey = "MaxExecutionTurnAnticipation";
        const string ExecutionTurnAnticipationKey = "ExecutionTurnAnticipation";
        const string MaxRetriesKey = "MaxRetries";

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
            var playersCount = GetConfigOption(config, PlayersCountKey, 2);
            var startDelay = GetConfigOption(config, StartDelayKey, 3000);
            var lsConfig = new LockstepConfig
            {
                CommandStepFactor = GetConfigOption(config, CommandStepFactorKey, LockstepConfig.DefaultCommandStepFactor),
                SimulationStep = GetConfigOption(config, SimulationStepKey, LockstepConfig.DefaultSimulationStep),
                MinExecutionTurnAnticipation = GetConfigOption(config, MinExecutionTurnAnticipationKey, LockstepConfig.DefaultMinExecutionTurnAnticipation),
                MaxExecutionTurnAnticipation = GetConfigOption(config, MaxExecutionTurnAnticipationKey, LockstepConfig.DefaultMaxExecutionTurnAnticipation),
                ExecutionTurnAnticipation = GetConfigOption(config, ExecutionTurnAnticipationKey, LockstepConfig.DefaultExecutionTurnAnticipation),
                MaxRetries = GetConfigOption(config, MaxRetriesKey, LockstepConfig.DefaultMaxRetries),
            };
            _netServer = new ServerLockstepNetworkController(
                this, lsConfig, playersCount, startDelay);
            _netServer.Init(
                new ServerLockstepController(_updateScheduler, lsConfig.CommandStep),
                _factory);

            _updateInterval = (float)lsConfig.CommandStep/1000.0f;
            _timer = PluginHost.CreateTimer(Update, 0, lsConfig.CommandStep);
            return true;
        }

        void Update()
        {
            _updateScheduler.Update(_updateInterval);
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
            return (int)TimeUtils.Timestamp;
        }
    }
}
