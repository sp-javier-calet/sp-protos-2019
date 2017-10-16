// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Connected.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   The connected.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

#define RETRY_CONNECTION_ALWAYS

namespace Photon.Stardust.S2S.Server.ConnectionStates.LoadBalancing
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using ExitGames.Logging;

    using Photon.SocketServer;
    using Photon.Stardust.S2S.Server.ClientConnections;
    using Photon.Stardust.S2S.Server.Diagnostics;
    using Photon.Stardust.S2S.Server.Enums;

    using PhotonHostRuntimeInterfaces;

    /// <summary>
    /// The connected.
    /// </summary>
    internal class AuthenticatedOnMaster : ConnectionStateBase
    {
        /// <summary>
        /// The instance.
        /// </summary>
        public static readonly AuthenticatedOnMaster Instance = new AuthenticatedOnMaster();

        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private static Random random = new Random();

        public static Hashtable LobbyCreateProperties = new Hashtable
            {
                { "C0", 10 },
                { "C1", 100 },
                { "C2", 1000 },
                { "C5", "Name" },
                { "C6", "Foo" },
                { "C7", "Bar" },
                { "LobbyProps", new byte[Settings.ReliableDataSize] },
                { "UpdateGame", 0 },
                { LoadBalancingGameCode.MaxPlayer, Settings.NumClientsPerGame },
                { LoadBalancingGameCode.LobbyProperties, new[] { "LobbyProps" } }
            };

        public static Hashtable DefaultLobbyCreateProperties = new Hashtable
            {
                { LoadBalancingGameCode.MaxPlayer, Settings.NumClientsPerGame },
            };

       
        public static string[] SqlLobbyJoinProperties = new string[]
            {
                "C0=10 AND C1=100 AND C5 != 'BLUBB'", "C0=10", "C1>99", "C5='Name' AND C6='Foo' AND C7='Bar'", "C5 LIKE 'N%'",
                "C7='BarXX'", "C0=1", "C7=B","C7 LIKE '%x%'", "C0=C1-90"
            };

        #region Implemented Interfaces

        #region IConnectionState


        public override void EnterState(ClientConnection client)
        {
            this.OpJoinLobby(client);
            Counters.ReliableOperationsSent.Increment();
            WindowsCounters.ReliableOperationsSent.Increment();
        }

        public void JoinOrCreate(ClientConnection client)
        {
            if (client.Number == 0)
           {
               this.OpCreateGame(client); 
           }
           else
           {
               this.OpJoinGame(client); 
           }
            
            Counters.ReliableOperationsSent.Increment();
            WindowsCounters.ReliableOperationsSent.Increment();
        }

        public override void TransitState(ClientConnection client)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug("Disconnecting from Master");
            }

            client.State = DisconnectingFromMaster.Instance;
            client.State.EnterState(client);
        }


        public void OpJoinLobby(ClientConnection client)
        {
            var data = new Dictionary<byte, object>
                {
                    { (byte)LoadBalancingParameterCode.LobbyType, Settings.LobbyType }, 
                    { (byte)LoadBalancingParameterCode.LobbyName, client.LobbyName }, 
                };

            client.Peer.SendOperationRequest(new OperationRequest(
                (byte)LoadBalancingOperationCode.JoinLobby, data), new SendParameters { Unreliable = false, ChannelId = 0, Encrypted = Settings.UseEncryption });
        }

        public void OpCreateGame(ClientConnection client)
        {
            //var data = new Dictionary<byte, object>
            //    {
            //        { (byte)LoadBalancingParameterCode.GameId, client.GameName }, 
            //    };

            // data.Add((byte)Enums.LiteOpKey.GameProperties, LobbyCreateProperties);

            //client.Peer.SendOperationRequest(new OperationRequest(
            //    (byte)LoadBalancingOperationCode.CreateGame, data), new SendParameters { Unreliable = false, ChannelId = 0, Encrypted = Settings.UseEncryption });

            var gameProperties = new Hashtable();
            gameProperties[(byte)LoadBalancingGameCode.MaxPlayer] = Settings.NumClientsPerGame;

            var data = new Dictionary<byte, object>
                {
                    { (byte)LoadBalancingParameterCode.GameId, client.GameName },
                    { (byte)LiteOpKey.GameProperties, gameProperties },
                    { (byte)LiteOpKey.JoinMode, (byte)1}
                };

            client.Peer.SendOperationRequest(new OperationRequest(
                (byte)LoadBalancingOperationCode.JoinGame, data), new SendParameters { Unreliable = false, ChannelId = 0, Encrypted = Settings.UseEncryption });

        }

        public void OpJoinGame(ClientConnection client)
        {

            //var data = new Dictionary<byte, object> { { (byte)LoadBalancingParameterCode.GameId, client.GameName }, };

            //data.Add(Enums.LiteOpKey.Data, this.GetSqlLobbyFilter());

            //client.Peer.SendOperationRequest(
            //      new OperationRequest((byte)LoadBalancingOperationCode.JoinRandomGame, data),
            //      new SendParameters { Unreliable = false, ChannelId = 0, Encrypted = Settings.UseEncryption });
            var gameProperties = new Hashtable();
            gameProperties[(byte)LoadBalancingGameCode.MaxPlayer] = Settings.NumClientsPerGame;

            var data = new Dictionary<byte, object>
                {
                    { (byte)LoadBalancingParameterCode.GameId, client.GameName },
                    { (byte)LiteOpKey.GameProperties, gameProperties },
                    { (byte)LiteOpKey.JoinMode, (byte)1}
                };

            client.Peer.SendOperationRequest(new OperationRequest(
                (byte)LoadBalancingOperationCode.JoinGame, data), new SendParameters { Unreliable = false, ChannelId = 0, Encrypted = Settings.UseEncryption });

        }

        private object GetSqlLobbyFilter()
        {
            string filter = string.Empty;

            if (Settings.LobbyType == 2)
            {
                var next = random.Next(0, SqlLobbyJoinProperties.Length);
                filter = SqlLobbyJoinProperties[next];

                if (log.IsInfoEnabled)
                {
                    log.InfoFormat("Joining Lobby game with filter string {0}", filter);
                }
            }

            return filter;
        }

        public override void StopClient(ClientConnection client)
        {
            Counters.ConnectedClients.Decrement();
            WindowsCounters.ConnectedClients.Decrement();

            client.State = Disconnected.Instance; 
        }
        
        public override void OnOperationReturn(ClientConnection client, OperationResponse operationResponse)
        {
            Counters.ReceivedOperationResponse.Increment();
            WindowsCounters.ReceivedOperationResponse.Increment();

            switch (operationResponse.OperationCode)
            {
                case (byte)LoadBalancingOperationCode.JoinLobby:
                    if (operationResponse.ReturnCode == 0)
                    {
                        if (client.StayInLobby)
                        {
                            log.InfoFormat("Client {0} stays in Lobby {1}.", client.Number, client.LobbyName);
                        }
                        else
                        {
                            this.JoinOrCreate(client);
                        }
                    }
                    else
                    {
                        log.WarnFormat("OnOperationReturn: {0} failed: ReturnCode: {1} ({2}). Disconnecting...", Enum.GetName(typeof(LoadBalancingOperationCode), operationResponse.OperationCode), operationResponse.ReturnCode, operationResponse.DebugMessage);
                        client.Peer.Disconnect();
                    }
                    break; 

                case (byte)LoadBalancingOperationCode.CreateGame:
                    if (operationResponse.ReturnCode == 0)
                    {
                        var gameServerAddress = (string)operationResponse.Parameters[(byte)LoadBalancingParameterCode.Address]; 
                        client.GameServerAddress = gameServerAddress;

                        this.TransitState(client);
                    }
                    else
                    {
                        log.WarnFormat("OnOperationReturn: {0} failed: ReturnCode: {1} ({2}). Disconnecting...", Enum.GetName(typeof(LoadBalancingOperationCode), operationResponse.OperationCode), operationResponse.ReturnCode, operationResponse.DebugMessage);
                        client.Peer.Disconnect();
                    }
                    break;

                case (byte)LoadBalancingOperationCode.JoinGame:
                case (byte)LoadBalancingOperationCode.JoinRandomGame:
                    if (operationResponse.ReturnCode == 0)
                    {
                        var gameServerAddress = (string)operationResponse.Parameters[(byte)LoadBalancingParameterCode.Address]; 
                        client.GameServerAddress = gameServerAddress;
//                        client.GameName = (string)operationResponse.Parameters[(byte)LoadBalancingParameterCode.GameId];
                        
                        log.InfoFormat("OnOperationReturn: JoinGame succeeded - going to join room {0} on GS.", client.GameName);

                        this.TransitState(client);
                    }
                    else
                    {
#if RETRY_CONNECTION_ALWAYS
                        log.WarnFormat(
                                "OnOperationReturn: {0} failed for client #{5}: ReturnCode: {1} ({2}). RetryCount: {3} of {4}. Trying again...",
                                Enum.GetName(typeof(LoadBalancingOperationCode), operationResponse.OperationCode),
                                operationResponse.ReturnCode,
                                operationResponse.DebugMessage,
                                client.MatchmakingRetryCount,
                                ClientConnection.MaxMatchmakingRetries,
                                client.Number);

                        client.Fiber.Schedule(() => this.JoinOrCreate(client), Settings.StartupInterval);
#else
                        if (client.MatchmakingRetryCount < ClientConnection.MaxMatchmakingRetries)
                        {
                            log.WarnFormat(
                                "OnOperationReturn: {0} failed for client #{5}: ReturnCode: {1} ({2}). RetryCount: {3} of {4}. Trying again...",
                                Enum.GetName(typeof(LoadBalancingOperationCode), operationResponse.OperationCode),
                                operationResponse.ReturnCode,
                                operationResponse.DebugMessage,
                                client.MatchmakingRetryCount,
                                ClientConnection.MaxMatchmakingRetries,
                                client.Number); 

                            client.Fiber.Schedule(
                                () => this.JoinOrCreate(client), Settings.StartupInterval * ++client.MatchmakingRetryCount);
                        }
                        else
                        {
                           log.WarnFormat(
                                "OnOperationReturn: {0} failed: ReturnCode: {1} ({2}). JoinLobby and start from beginning again...",
                                Enum.GetName(typeof(LoadBalancingOperationCode), operationResponse.OperationCode),
                                operationResponse.ReturnCode,
                                operationResponse.DebugMessage);
                           //client.Peer.Disconnect();

                           this.EnterState(client);
                        }
#endif
                    }
                    break;

                default:
                    {
                        log.WarnFormat("OnOperationReturn: unexpected return code {0} of operation {1}", operationResponse.ReturnCode, operationResponse.OperationCode);
                        break;
                    }
            }
        }

        /// <summary>
        /// The on peer status callback.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="returnCode">
        /// The return code.
        /// </param>
        public override void OnPeerStatusCallback(ClientConnection client, DisconnectReason returnCode)
        {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("{0}", returnCode);
            }

            Counters.ConnectedClients.Decrement();
            WindowsCounters.ConnectedClients.Decrement();

            client.State = Disconnected.Instance;

            client.OnDisconnected();
        }

        /// <summary>
        /// The on update.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        public override void OnUpdate(ClientConnection client)
        {
           client.EnqueueUpdate();
        }

        #endregion

        #endregion
    }
}