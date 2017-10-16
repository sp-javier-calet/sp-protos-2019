namespace Photon.Stardust.S2S.Server
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    using ExitGames.Concurrency.Fibers;
    using ExitGames.Logging;
    using ExitGames.Logging.Log4Net;
    using ExitGames.Threading;

    using log4net;
    using log4net.Config;

    using Photon.SocketServer;
    using Photon.Stardust.S2S.Server.ClientConnections;
    using Photon.Stardust.S2S.Server.Diagnostics;

    using LogManager = ExitGames.Logging.LogManager;

    public class Application : ApplicationBase
    {
        #region Constants and Fields
        private const bool UseStatsLogger = false;

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The base game name.
        /// </summary>
        private static readonly string baseGameName = string.Format("{0}({1})", Environment.MachineName, Process.GetCurrentProcess().Id);

        /// <summary>
        /// The fiber.
        /// </summary>
        private static readonly PoolFiber fiber = new PoolFiber(new FailSafeBatchExecutor());

        private static readonly PoolFiber counterLoggingFiber = new PoolFiber(new FailSafeBatchExecutor());

         /// <summary>
        /// The games.
        /// </summary>
        private static readonly Dictionary<string, List<ClientConnection>> games = new Dictionary<string, List<ClientConnection>>();

        /// <summary>
        /// The lobbies with their peers (that stay in the lobby and don`t actually create games 
        /// </summary>
        private static readonly Dictionary<string, List<ClientConnection>> lobbies = new Dictionary<string, List<ClientConnection>>();
        
        /// <summary>
        /// The reset event.
        /// </summary>
        private static readonly ManualResetEvent resetEvent = new ManualResetEvent(false);

        /// <summary>
        /// The game counter.
        /// </summary>
        private static int gameCounter;

        /// <summary>
        /// The client counter.
        /// </summary>
        private static int clientCounter;



        /// <summary>
        /// The stopped.
        /// </summary>
        private static bool stopped;

        #endregion

        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
            // controller peer
            throw new NotImplementedException("No client connections to this app allowed");
        }


        protected override void Setup()
        {
            InitializeLog4NetForServerApp(this.BinaryPath, this.PhotonInstanceName, this.ApplicationRootPath);
            WindowsCounters.Initialize();

            log.InfoFormat("Settings: {0} lobbies per process, {1} games per lobby, {2} players per game, {3} additional players per lobby, game server at {3}",
                Settings.NumLobbies,
                Settings.NumGamesPerLobby,
                Settings.NumClientsPerGame,
                Settings.NumClientsPerLobby,
                Settings.ServerAddress);

            if (Settings.SendReliableData)
            {
                log.InfoFormat("Sending reliable operation every {0} ms", Settings.ReliableDataSendInterval);
            }
            if (Settings.SendUnreliableData)
            {
                log.InfoFormat("Sending unreliable operation every {0} ms", Settings.UnreliableDataSendInterval);
            }


            stopped = false;
            fiber.Start();
            counterLoggingFiber.Start();

            if(UseStatsLogger && StatsLogger.IsValid())
            {
                StatsLogger.Initialize();
                counterLoggingFiber.ScheduleOnInterval(StatsLogger.Print, 0, Settings.LogCounterInterval);
            }

            counterLoggingFiber.ScheduleOnInterval(CounterLogger.PrintCounter, Settings.LogCounterInterval, Settings.LogCounterInterval);

            try
            {
                log.InfoFormat("Starting {0} games with {1} players", Settings.NumGamesPerLobby * Settings.NumLobbies, Settings.NumClientsPerGame);
                for (int l = 0; l < Settings.NumLobbies; l++)
                {
                    string lobbyName = "Default"; 
                    if (l > 0)
                    {
                        lobbyName = string.Format("L_{0}_{1}", baseGameName, l);
                    }

                    for (int g = 0; g < Settings.NumGamesPerLobby; g++)
                    {
                        this.StartGame(lobbyName, false); 
                    }

                    this.CreateLobbyClients(lobbyName); 
                }

                log.Info("Finished clients start");
            }
            catch (Exception e)
            {
                log.Error(e);
            }
            finally
            {
                log.InfoFormat("[{0}] Stopped to create games and clients", Process.GetCurrentProcess().Id);
            }
            
        }

        protected override void TearDown()
        {
            log.InfoFormat("Stopping {0} games", games.Count);
            StopGames();
            fiber.Stop();

            // wait for stop to complete
            resetEvent.WaitOne();
        }


        public static void InitializeLog4NetForServerApp(string binaryPath, string photonInstanceName, string applicationRootPath)
        {
            GlobalContext.Properties["Photon:InstanceName"] = photonInstanceName;
            GlobalContext.Properties["Photon:ApplicationLogPath"] = Path.Combine(applicationRootPath, "log");

            LogManager.SetLoggerFactory(Log4NetLoggerFactory.Instance);
            var configFileInfo = new FileInfo(Path.Combine(binaryPath, "log4net.config"));
            XmlConfigurator.ConfigureAndWatch(configFileInfo);
        }


        private void CreateLobbyClients(string lobbyName)
        {
            var clients = new List<ClientConnection>(Settings.NumClientsPerLobby);
            lobbies.Add(lobbyName, clients);

            for (int i = 1; i <= Settings.NumClientsPerLobby; i++)
            {
                var x = i; 
                
                ++clientCounter;
                // don't start all at once, that would not be realistic
                fiber.Schedule(
                    () =>
                        {
                            var client = new ClientConnection(string.Empty, lobbyName, x, true, this);

                            client.Start();
                            clients.Add(client);
                        },
                    Settings.StartupInterval * clientCounter);
            }
        }

        /// <summary>
        /// The start game.
        /// </summary>
        private void StartGame(string lobbyName, bool startImmediately)
        {
            if (stopped)
            {
                return;
            }
            
            gameCounter++;
            string gameName = string.Format("{0}_G_{1}_{2}", lobbyName, baseGameName, gameCounter);

            var clients = new List<ClientConnection>(Settings.NumClientsPerGame);
            games.Add(gameName, clients);

            for (int i = 1; i <= Settings.NumClientsPerGame; i++)
            {
                ++clientCounter;

                var x = i;

                int startupTime = startImmediately
                                      ? Settings.StartupInterval * i
                                      : Settings.StartupInterval * clientCounter; 

                // don't start all at once, that would not be realistic
                fiber.Schedule(
                    () =>
                        {
                            var client = CreateClientConnection(gameName, lobbyName, x);

                            client.Start();
                            clients.Add(client);
                        },
                        startupTime);
            }

            log.InfoFormat("[{2}] Started game {1} with {0} clients", Settings.NumClientsPerGame, gameName, Process.GetCurrentProcess().Id);

            if (Settings.TimeInGame > 0)
            {
                fiber.Schedule(() => this.StopGame(gameName), (Settings.StartupInterval * clientCounter) + (long)TimeSpan.FromSeconds(Settings.TimeInGame).TotalMilliseconds);
            }
        }

        protected virtual ClientConnection CreateClientConnection(string gameName, string lobbyName, int num)
        {
            return new ClientConnection(gameName, lobbyName, num, false, this);
        }

        /// <summary>
        /// The stop game.
        /// </summary>
        /// <param name="gameName">
        /// The game name.
        /// </param>
        private void StopGame(string gameName)
        {
            string lobbyName = null; 
            List<ClientConnection> clients;
            if (games.TryGetValue(gameName, out clients))
            {
                if (clients.Count > 0)
                {
                    lobbyName = clients[0].LobbyName; 
                }
                clients.ForEach(c => c.Stop());
                clients.Clear();
                games.Remove(gameName);
                log.InfoFormat("[{1}] Stopped game {0}", gameName, Process.GetCurrentProcess().Id);
            }

            this.StartGame(lobbyName, true);
        }

        /// <summary>
        /// The stop games.
        /// </summary>
        private static void StopGames()
        {
            try
            {
                if (games != null)
                {
                    foreach (KeyValuePair<string, List<ClientConnection>> entry in games)
                    {
                        entry.Value.ForEach(c => c.Stop());
                        entry.Value.Clear();
                        log.InfoFormat("[{1}] Shutdown: Stopped game {0}", entry.Key, Process.GetCurrentProcess().Id);
                    }

                    games.Clear();
                }
            }
            finally
            {
                stopped = true;
                resetEvent.Set();
            }
        }
    }
}
