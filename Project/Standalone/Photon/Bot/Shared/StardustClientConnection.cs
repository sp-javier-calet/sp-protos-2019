
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
        StardustApplication _app;
        public UpdateScheduler Scheduler { get; private set; }
        public StardustNetworkClient NetworkClient { get; private set; }
        const float UpdateInterval = ((float)UpdateIntervalMillis) / 1000.0f;

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public StardustClientConnection(string gameName, string lobbyName, int number, bool stayInLobby, StardustApplication application)
            : base(gameName, lobbyName, number, stayInLobby, application)
        {
            _app = application;
            Scheduler = new UpdateScheduler();
        }

        public override void Update()
        {
            base.Update();
            Scheduler.Update(UpdateInterval, UpdateInterval);
        }

        protected override GamingPeer CreateGamingPeer()
        {
            NetworkClient = new StardustNetworkClient(this, Application);
            _gameClient = CreateGameClient();
            return NetworkClient;
        }

        const string GameTypeSetting = "GameType";
        const string GameAssemblySetting = "GameAssembly";

        object CreateGameClient()
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
                var factory = CreateInstanceFromAssembly(gameAssembly, gameType);
                return _app.CreateGameClient(factory, this, config);
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
            var gameType = Assembly.LoadFrom(path).GetType(typeName);
            return Activator.CreateInstance(gameType);
        }
    }
}