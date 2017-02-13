// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WaitingForConnect.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   The waiting for connect.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Photon.Stardust.S2S.Server.ConnectionStates.LoadBalancing
{
    using System;

    using ExitGames.Logging;

    using Photon.Stardust.S2S.Server.ClientConnections;
    using Photon.Stardust.S2S.Server.Diagnostics;

    using PhotonHostRuntimeInterfaces;

    internal class WaitingForMasterConnect : ConnectionStateBase
    {
        /// <summary>
        /// The instance.
        /// </summary>
        public static readonly WaitingForMasterConnect Instance = new WaitingForMasterConnect();

        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        #region Implemented Interfaces

        #region IConnectionState
        public override void EnterState(ClientConnection client)
        {
            if (client.ConnectToServer())
            {
                client.EnqueueUpdate();
            }
            else
            {
                throw new InvalidOperationException("connect failed");
            }
        }

        public override void TransitState(ClientConnection client)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug("Connected to Master");
            }

            client.State = ConnectedToMaster.Instance;
            
            Counters.ConnectedClients.Increment();
            WindowsCounters.ConnectedClients.Increment();

            client.State.EnterState(client);
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

        #endregion

        #endregion
    }
}