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
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        const string CommandStepDurationConfig = "CommandStepDuration";
        const string SimulationStepDurationConfig = "SimulationStepDuration";
        const string MaxPlayersConfig = "MaxPlayers";
        const string ClientStartDelayConfig = "ClientStartDelay";
        const string ClientSimulationDelayConfig = "ClientSimulationDelay";
        const string BattleEndedWithoutConfirmationTimeoutConfig = "BattleEndedWithoutConfirmationTimeout";
        const string FinishOnClientDisconnectionConfig = "FinishOnClientDisconnection";
        const string AllowBattleStartWithOnePlayerReadyConfig = "AllowBattleStartWithOnePlayerReady";
        const string GameAssemblyNameConfig = "GameAssemblyName";
        const string GameTypeConfig = "GameType";
        const string MetricSendIntervalConfig = "MetricSendInterval";

        public override bool SetupInstance(IPluginHost host, Dictionary<string, string> config, out string errorMsg)
        {
            if (!base.SetupInstance(host, config, out errorMsg))
            {
                return false;
            }

            Func<string> getBaseUrlCallback = () => { return BaseBackendUrl; };

            _matchmaking = new HttpMatchmakingServer(new ImmediateWebRequestHttpClient(), getBaseUrlCallback);
            _netServer = new LockstepNetworkServer(this, _matchmaking);

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
            _netServer.ServerConfig.MatchEndedWithoutConfirmationTimeout = GetConfigOption(config,
                BattleEndedWithoutConfirmationTimeoutConfig, _netServer.ServerConfig.MatchEndedWithoutConfirmationTimeout);
            _netServer.ServerConfig.FinishOnClientDisconnection = GetConfigOption(config, FinishOnClientDisconnectionConfig, _netServer.ServerConfig.FinishOnClientDisconnection);
            _netServer.ServerConfig.MetricSendInterval = GetConfigOption(config,
                MetricSendIntervalConfig, _netServer.ServerConfig.MetricSendInterval);

            _netServer.ServerLockstep.MetricSendInterval = _netServer.ServerConfig.MetricSendInterval;
            _netServer.ServerConfig.GetBackendUrlCallback = getBaseUrlCallback;
            config.TryGetValue(MetricEnvironmentConfig, out _netServer.ServerConfig.MetricEnvironment);

            if (PluginEventTracker != null)
            {
                PluginEventTracker.Environment = _netServer.ServerConfig.MetricEnvironment;
                PluginEventTracker.GetBaseUrlCallback = _netServer.ServerConfig.GetBackendUrlCallback;
                PluginEventTracker.Platform = "PhotonPlugin";
                PluginEventTracker.UpdateCommonTrackData += (data) => { data.SetValue("ver", AppVersion); };
            }

            _netServer.ServerConfig.AllowMatchStartWithOnePlayerReady = GetConfigOption(config,
                AllowBattleStartWithOnePlayerReadyConfig, _netServer.ServerConfig.AllowMatchStartWithOnePlayerReady);

            string gameAssembly;
            string gameType;
            if (config.TryGetValue(GameAssemblyNameConfig, out gameAssembly) &&
                config.TryGetValue(GameTypeConfig, out gameType))
            {
                try
                {
                    var factory = (INetworkServerGameFactory)CreateInstanceFromAssembly(gameAssembly, gameType);
                    _game = factory.Create(_netServer, config);
                }
                catch (Exception e)
                {
                    errorMsg = e.Message;
                }
            }
            return string.IsNullOrEmpty(errorMsg);
        }

        protected override void Update(float dt)
        {
            base.Update(dt);
            _netServer.Update();
        }

    }
}
