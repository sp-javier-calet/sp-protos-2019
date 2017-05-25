﻿using SocialPoint.Network;
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
        bool _isURLEditable;

        public LockstepPlugin() : base("Lockstep")
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        void OnBeforeMatchStarts()
        {
            if(_isURLEditable)
            {
                _matchmaking.BaseUrl = BackendEnv;
            }
        }

        const string CommandStepDurationConfig = "CommandStepDuration";
        const string SimulationStepDurationConfig = "SimulationStepDuration";
        const string MaxPlayersConfig = "MaxPlayers";
        const string ClientStartDelayConfig = "ClientStartDelay";
        const string ClientSimulationDelayConfig = "ClientSimulationDelay";
        const string BackendBaseUrlConfig = "BackendBaseUrl";
        const string BattleEndedWithoutConfirmationTimeoutConfig = "BattleEndedWithoutConfirmationTimeout";
        const string FinishOnClientDisconnectionConfig = "FinishOnClientDisconnection";
        const string AllowBattleStartWithOnePlayerReadyConfig = "AllowBattleStartWithOnePlayerReady";
        const string IsURLEditableConfig = "IsURLEditable";
        const string GameAssemblyNameConfig = "GameAssemblyName";
        const string GameTypeConfig = "GameType";
        const string MetricSendIntervalConfig = "MetricSendInterval";

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
            _netServer.ServerConfig.BattleEndedWithoutConfirmationTimeout = GetConfigOption(config,
                BattleEndedWithoutConfirmationTimeoutConfig, _netServer.ServerConfig.BattleEndedWithoutConfirmationTimeout);
            _netServer.ServerConfig.FinishOnClientDisconnection = GetConfigOption(config, FinishOnClientDisconnectionConfig, _netServer.ServerConfig.FinishOnClientDisconnection);
            _netServer.ServerConfig.MetricSendInterval = GetConfigOption(config,
                MetricSendIntervalConfig, _netServer.ServerConfig.MetricSendInterval);
            
            _netServer.ServerLockstep.MetricSendInterval = _netServer.ServerConfig.MetricSendInterval;

            string baseUrl;
            config.TryGetValue(BackendBaseUrlConfig, out baseUrl);
            if (_matchmaking != null && !string.IsNullOrEmpty(baseUrl))
            {
                _matchmaking.BaseUrl = baseUrl;
            }
            if(PluginEventTracker != null && baseUrl != string.Empty)
            {
                PluginEventTracker.BaseUrl = baseUrl;
            }

            _netServer.ServerConfig.AllowBattleStartWithOnePlayerReady = GetConfigOption(config,
                AllowBattleStartWithOnePlayerReadyConfig, _netServer.ServerConfig.AllowBattleStartWithOnePlayerReady);
            _isURLEditable = GetConfigOption(config, IsURLEditableConfig, _isURLEditable);

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
