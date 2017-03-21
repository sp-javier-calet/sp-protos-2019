// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthenticatedOnGameServer.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   The connected.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

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
    internal class AuthenticatedOnGameServer : ConnectionStateBase
    {
        /// <summary>
        /// The instance.
        /// </summary>
        public static readonly AuthenticatedOnGameServer Instance = new AuthenticatedOnGameServer();

        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        #region Implemented Interfaces

        #region IConnectionState

    

        public override void EnterState(ClientConnection client)
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
                log.Debug("Playing");
            }

            client.State = Playing.Instance;
            client.StartTimers();
        }
        
        public void OpCreateGame(ClientConnection client)
        {
            var gameProperties = new Hashtable();
            gameProperties[(byte)LoadBalancingGameCode.MaxPlayer] = Settings.NumClientsPerGame;

            var data = new Dictionary<byte, object>
                {
                    { (byte)LoadBalancingParameterCode.GameId, client.GameName },
                    { (byte)LiteOpKey.GameProperties, gameProperties },
                    { (byte)LiteOpKey.JoinMode, (byte)1 },
                    { (byte)LiteOpKey.Plugins, Settings.Plugins }
                };

            client.Peer.SendOperationRequest(new OperationRequest(
                (byte)LoadBalancingOperationCode.JoinGame, data), new SendParameters { Unreliable = false, ChannelId = 0, Encrypted = Settings.UseEncryption });
        }

        public void OpJoinGame(ClientConnection client)
        {
            //var data = new Dictionary<byte, object>
            //    {
            //        { (byte)LoadBalancingParameterCode.GameId, client.GameName }, 
            //    };

            //client.Peer.SendOperationRequest(new OperationRequest(
            //    (byte)LoadBalancingOperationCode.JoinGame, data), new SendParameters { Unreliable = false, ChannelId = 0, Encrypted = Settings.UseEncryption });

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
                case (byte)LoadBalancingOperationCode.JoinGame:
                case (byte)LoadBalancingOperationCode.CreateGame:
                    if (operationResponse.ReturnCode == 0)
                    {
                        var actorNumber = (int)operationResponse.Parameters[(byte)LiteOpKey.ActorNr];
                        var gameProperties = (Hashtable)operationResponse.Parameters[(byte)LiteOpKey.GameProperties];
                      
                        log.InfoFormat("Joined / Created Game. Actor number: {0}, MaxPlayers: {1}", actorNumber, gameProperties[(byte)LoadBalancingGameCode.MaxPlayer]);
                        
                        this.TransitState(client);
                    }
                    else
                    {
                        log.WarnFormat("OnOperationReturn: {0} failed: ReturnCode: {1} ({2}). Disconnecting...", Enum.GetName(typeof(LoadBalancingOperationCode), operationResponse.OperationCode), operationResponse.ReturnCode, operationResponse.DebugMessage);
                        client.Peer.Disconnect();
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
        /// The on update.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        public override void OnUpdate(ClientConnection client)
        {
            client.EnqueueUpdate();
        }


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

        #endregion

        #endregion
    }
}