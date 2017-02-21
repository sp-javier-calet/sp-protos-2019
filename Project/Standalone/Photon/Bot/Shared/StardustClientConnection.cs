
using Photon.Stardust.S2S.Server;
using Photon.Stardust.S2S.Server.ClientConnections;
using ExitGames.Logging;
using System.Configuration;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System;
using SocialPoint.Utils;

namespace SocialPoint.Network 
{
    public class StardustClientConnection : ClientConnection
    {
        object _gameClient;
        UpdateScheduler _updateScheduler;
        const float UpdateInterval = ((float)UpdateIntervalMillis) / 1000.0f;

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public StardustClientConnection(string gameName, string lobbyName, int number, bool stayInLobby, Application application)
            : base(gameName, lobbyName, number, stayInLobby, application)
        {
            _updateScheduler = new UpdateScheduler();
        }

        public override void Update()
        {
            base.Update();
            _updateScheduler.Update(UpdateInterval);
        }

        protected override GamingPeer CreateGamingPeer()
        {
            var peer = new StardustNetworkClient(this, Application);
            _gameClient = CreateGameClient(peer);
            return peer;
        }

        const string GameTypeSetting = "GameType";
        const string GameAssemblySetting = "GameAssembly";

        object CreateGameClient(INetworkClient netClient)
        {
            var config = new Dictionary<string, string>();
            var keys = ConfigurationManager.AppSettings.AllKeys;
            for(var i=0; i<keys.Length; i++)
            {
                var k = keys[i];
                config[k] = ConfigurationManager.AppSettings[k];
            }

            string gameType;
            if(!config.TryGetValue(GameTypeSetting, out gameType))
            {
                return null;
            }
            string gameAssembly;
            if(!config.TryGetValue(GameAssemblySetting, out gameAssembly))
            {
                return null;
            }

            try
            {
                var factory = (INetworkClientGameFactory)CreateInstanceFromAssembly(
                    gameAssembly, gameType);
                return factory.Create(netClient, _updateScheduler, config);
            }
            catch(Exception e)
            {
                log.ErrorFormat("exception creating game client %s", e);
            }

            return null;
        }

        protected object CreateInstanceFromAssembly(string assemblyName, string typeName)
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(dir, assemblyName);
            var gameType = Assembly.LoadFile(path).GetType(typeName);
            return Activator.CreateInstance(gameType);
        }
    }
}