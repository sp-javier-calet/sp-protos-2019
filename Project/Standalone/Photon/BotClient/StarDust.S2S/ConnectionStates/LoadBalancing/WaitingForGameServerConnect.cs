// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WaitingForGameServerConnect.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Photon.Stardust.S2S.Server.ConnectionStates.LoadBalancing
{
    using System;

    using ExitGames.Logging;

    using Photon.Stardust.S2S.Server.ClientConnections;
    using Photon.Stardust.S2S.Server.Diagnostics;

    using PhotonHostRuntimeInterfaces;

    public class WaitingForGameServerConnect : ConnectionStateBase
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public static WaitingForGameServerConnect Instance = new WaitingForGameServerConnect();  

        public override void EnterState(ClientConnection client)
        {
            string gameServerAddress = client.GameServerAddress;
            
            if (client.Peer != null && client.Peer.Connected)
            {
                log.WarnFormat("Could not connect to GS: Peer {0} is not disconencted. Stopping client...", client.Peer.ConnectionId);
                client.Peer.Disconnect();
            }
            else
            {
                if (client.ConnectToServer(gameServerAddress, Settings.Protocol, "Game"))
                {
                    log.InfoFormat("Connecting to " + gameServerAddress);
                    this.OnUpdate(client, 0);
                }
                else
                {
                    throw new InvalidOperationException("connect failed to " + gameServerAddress);
                }
            }
        }

        public override void OnPeerStatusCallback(ClientConnection client, DisconnectReason returnCode)
        {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("{0}", returnCode);
            }

            client.OnDisconnected();
        }


        /// <summary>
        /// The on update.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        public override void OnUpdate(ClientConnection client, int elapsedMiliSeconds)
        {
            client.EnqueueUpdate();
        }

        public override void TransitState(ClientConnection client)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug("Connected to GameServer");
            }

            client.State = ConnectedToGameServer.Instance;

            Counters.ConnectedClients.Increment();
            WindowsCounters.ConnectedClients.Increment();

            client.State.EnterState(client);
        }
    }
}
