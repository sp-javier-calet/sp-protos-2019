using SocialPoint.Network;
using SocialPoint.Matchmaking;
using System;
using System.Collections.Generic;
using Photon.Hive.Plugin;
using SocialPoint.Network.ServerEvents;

namespace SocialPoint.Lockstep
{
    /*
     * set the current public server IP address in this file:
     * deploy/LoadBalancing/GameServer/bin/Photon.LoadBalancing.dll.config
     */
    public class LockstepPlugin : NetworkServerPlugin
    {
        protected override bool Full
        {
            get
            {
                return _netServer.Full;
            }
        }

        protected override int MaxPlayers
        {
            get
            {
                return _netServer.MaxPlayers;
            }
        }

        protected override int UpdateInterval
        {
            get
            {
                return _netServer.Config.CommandStepDuration;
            }
        }

        LockstepNetworkServer _netServer;
        HttpMatchmakingServer _matchmaking;
        object _game;

        public LockstepPlugin() : base("Lockstep")
        {
        }

        void OnBeforeMatchStarts()
        {
            var backendEnv = BackendEnv;
            if(!string.IsNullOrEmpty(backendEnv))
            {
                _matchmaking.BaseUrl = string.Format(_matchmaking.BaseUrl, backendEnv);
            }
        }

        const string CommandStepDurationConfig = "CommandStepDuration";
        const string SimulationStepDurationConfig = "SimulationStepDuration";
        const string MaxPlayersConfig = "MaxPlayers";
        const string ClientStartDelayConfig = "ClientStartDelay";
        const string ClientSimulationDelayConfig = "ClientSimulationDelay";
        const string BackendBaseUrlConfig = "BackendBaseUrl";
        const string GameAssemblyNameConfig = "GameAssemblyName";
        const string GameTypeConfig = "GameType";

        public override bool SetupInstance(IPluginHost host, Dictionary<string, string> config, out string errorMsg)
        {
            if(!base.SetupInstance(host, config, out errorMsg))
            {
                return false;
            }

            _matchmaking = new HttpMatchmakingServer(new ImmediateWebRequestHttpClient());
            _netServer = new LockstepNetworkServer(NetworkServer, _matchmaking);
            _netServer.BeforeMatchStarts += OnBeforeMatchStarts;

            _netServer.SendMetric = PluginEventTracker.SendMetric;
            _netServer.SendLog = PluginEventTracker.SendLog;
            _netServer.SendTrack = PluginEventTracker.SendTrack;

            _matchmaking.Version = AppVersion;
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
            config.TryGetValue(BackendBaseUrlConfig, out baseUrl);
            if (_matchmaking != null && baseUrl != string.Empty)
            {
                _matchmaking.BaseUrl = baseUrl;
            }
            if(PluginEventTracker != null && baseUrl != string.Empty)
            {
                PluginEventTracker.BaseUrl = baseUrl;
            }

            string gameAssembly;
            string gameType;
            if(config.TryGetValue(GameAssemblyNameConfig, out gameAssembly) &&
                config.TryGetValue(GameTypeConfig, out gameType))
            {
                try
                {
                    var factory = (INetworkServerGameFactory)CreateInstanceFromAssembly(gameAssembly, gameType);
                    _game = factory.Create(_netServer, config);
                }
                catch(Exception e)
                {
                    errorMsg = e.Message;
                }
            }
            return string.IsNullOrEmpty(errorMsg);
        }

        protected override void Update()
        {
            base.Update();
            _netServer.Update();
        }

    }
}
