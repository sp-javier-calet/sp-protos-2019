using System;
using System.Collections.Generic;
using Photon.Hive.Plugin;
using SocialPoint.Matchmaking;
using SocialPoint.Network;

namespace SocialPoint.Multiplayer
{
    public class AuthoritativePlugin : NetworkServerPlugin
    {
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
        HttpMatchmakingServer _matchmaking;

        object _game;
        int _maxPlayers = 6;
        int _currentPlayers;
        int _lastUpdateTimestamp;
        int _updateInterval = 70;

        public AuthoritativePlugin(string name="Authoritative") : base(name)
        {
            _netServer = new NetworkServerSceneController(this);
            _matchmaking = new HttpMatchmakingServer(new ImmediateWebRequestHttpClient(), () => { return BaseBackendUrl + "/matchmaking"; });
            _lastUpdateTimestamp = ((INetworkServer)this).GetTimestamp();
        }

        const string MaxPlayersConfig = "MaxPlayers";
        const string GameAssemblyNameConfig = "GameAssemblyName";
        const string GameTypeConfig = "GameType";

        public override bool SetupInstance(IPluginHost host, Dictionary<string, string> config, out string errorMsg)
        {
            if(!base.SetupInstance(host, config, out errorMsg))
            {
                return false;
            }

            _maxPlayers = (byte)GetConfigOption(config, MaxPlayersConfig, MaxPlayers);

            string gameAssembly;
            string gameType;
            if (config.TryGetValue(GameAssemblyNameConfig, out gameAssembly) && config.TryGetValue(GameTypeConfig, out gameType))
            {
                try
                {
                    var factory = (INetworkServerGameFactory)CreateInstanceFromAssembly(gameAssembly, gameType);
                    _game = factory.Create(_netServer, this, _fileManager, _matchmaking, config);
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
