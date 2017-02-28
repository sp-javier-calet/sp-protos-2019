// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DisconnectingFromMaster.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   The connected.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Photon.Stardust.S2S.Server.ConnectionStates.LoadBalancing
{
    using ExitGames.Logging;

    using Photon.Stardust.S2S.Server.ClientConnections;
    using Photon.Stardust.S2S.Server.Diagnostics;

    using PhotonHostRuntimeInterfaces;

    /// <summary>
    /// The connected.
    /// </summary>
    internal class DisconnectingFromMaster : ConnectionStateBase
    {
        /// <summary>
        /// The instance.
        /// </summary>
        public static readonly DisconnectingFromMaster Instance = new DisconnectingFromMaster();

        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        #region Implemented Interfaces

        #region IConnectionState


        public override void EnterState(ClientConnection client)
        {
            client.Peer.Disconnect();
            
            Counters.ConnectedClients.Decrement();
            WindowsCounters.ConnectedClients.Decrement();
        }

        public override void TransitState(ClientConnection client)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug("Waiting for GameServer connect");
            }
               
            client.State = WaitingForGameServerConnect.Instance;
            client.State.EnterState(client); 
        }

        public override void StopClient(ClientConnection client)
        {
            Counters.ConnectedClients.Decrement();
            WindowsCounters.ConnectedClients.Decrement();

            client.State = Disconnected.Instance; 
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
            switch (returnCode)
            {
                case DisconnectReason.ManagedDisconnect:
                    // this is expected! automatically reconnect to GS, don't call OnDisconnect. 
                    if (log.IsInfoEnabled)
                    {
                        log.Info("Disconnected from Master - reconnecting to GS...");
                    }
                    this.TransitState(client);
                    break; 

                default:
                    {
                        if (log.IsDebugEnabled)
                        {
                            log.DebugFormat("{0}", returnCode);
                        }

                        Counters.ConnectedClients.Decrement();
                        WindowsCounters.ConnectedClients.Decrement();

                        client.OnDisconnected();
                        break;
                    }
            }
        }

        public override void OnUpdate(ClientConnection client)
        {
            client.EnqueueUpdate();
        }
        #endregion

        #endregion
    }
}