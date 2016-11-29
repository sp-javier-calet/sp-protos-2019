using System;
using System.Collections.Generic;
using SocialPoint.Multiplayer;
using SocialPoint.Network;
using SocialPoint.IO;
using System.IO;
using System.Collections;

namespace Photon.Hive.Plugin.Authoritative
{
    public class AuthoritativePlugin : NetworkServerPlugin
    {
        public override string Name
        {
            get
            {
                return "Authoritative";
            }
        }

        override protected int UpdateInterval
        {
            get
            {
                return _updateInterval;
            }
        }

        protected override bool Full
        {
            get
            {
                return PluginHost.GameActorsActive.Count >= MaxPlayers;
            }
        }

        protected override int MaxPlayers
        {
            get
            {
                return _maxPlayers;
            }
        }

        NetworkServerSceneController _netServer;
        object _game;
        int _maxPlayers = 4;
        int _currentPlayers = 0;
        int _lastUpdateTimestamp = 0;
        int _updateInterval = 100;

        public AuthoritativePlugin():base()
        {
            _netServer = new NetworkServerSceneController(this);
        }

        const string MaxPlayersConfig = "MaxPlayers";
        const string GameAssemblyNameConfig = "GameAssemblyName";
        const string GameTypeConfig = "GameType";

        public override bool SetupInstance(IPluginHost host, Dictionary<string, string> config, out string errorMsg)
        {
            if (!base.SetupInstance(host, config, out errorMsg))
            {
                return false;
            }

            _maxPlayers = (byte)GetConfigOption(config, MaxPlayersConfig, MaxPlayers);

            string gameAssembly;
            string gameType;
            if (config.TryGetValue(GameAssemblyNameConfig, out gameAssembly) &&
                config.TryGetValue(GameTypeConfig, out gameType))
            {
                try
                {
                    var factory = (INetworkServerGameFactory)CreateInstanceFromAssembly(gameAssembly, gameType);
                    _game = factory.Create(this, _netServer, config);
                }
                catch (Exception e)
                {
                    errorMsg = e.Message;
                }
            }

            return true;
        }

        override protected void OnClientConnected(byte clientId)
        {
            base.OnClientConnected(clientId);
            _currentPlayers++;
        }

        override protected void OnClientDisconnected(byte clientId)
        {
            _currentPlayers--;
            base.OnClientDisconnected(clientId);
        }

        protected override void Update()
        {
            float deltaTime = UpdateDeltaTime();
            _netServer.Update(deltaTime);
        }

        float UpdateDeltaTime()
        {
            int currentTimestamp = ((INetworkServer)this).GetTimestamp();
            float deltaTime = ((float)(currentTimestamp - _lastUpdateTimestamp)) * 0.001f;//Milliseconds to seconds
            _lastUpdateTimestamp = currentTimestamp;
            return deltaTime;
        }

    }
}
