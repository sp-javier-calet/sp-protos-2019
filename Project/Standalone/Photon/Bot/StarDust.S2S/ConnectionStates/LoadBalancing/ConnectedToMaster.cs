// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Connected.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   The connected.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Photon.Stardust.S2S.Server.ConnectionStates.LoadBalancing
{
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
    internal class ConnectedToMaster : ConnectionStateBase
    {
        /// <summary>
        /// The instance.
        /// </summary>
        public static readonly ConnectedToMaster Instance = new ConnectedToMaster();

        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        #region Implemented Interfaces

        #region IConnectionState
        public override void EnterState(ClientConnection client)
        {
            this.OpAuthenticate(client); 
            
            Counters.ReliableOperationsSent.Increment();
            WindowsCounters.ReliableOperationsSent.Increment();
        }

        public override void TransitState(ClientConnection client)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug("Authenticated on Master");
            }

            client.State = AuthenticatedOnMaster.Instance;
            client.State.EnterState(client);
        }

        private void OpAuthenticate(ClientConnection client)
        {
            var data = new Dictionary<byte, object> { { 224, Settings.ApplicationId } };


            client.Peer.SendOperationRequest(
                new OperationRequest((byte)LoadBalancingOperationCode.Authenticate, data),
                new SendParameters { Unreliable = false, ChannelId = 0, Encrypted = Settings.UseEncryption });
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
                case (byte)LoadBalancingOperationCode.Authenticate: 
                    if (operationResponse.ReturnCode == 0)
                    {
                       client.AuthenticationToken = (string)operationResponse[(byte)LoadBalancingParameterCode.Secret]; 
                       this.TransitState(client);
                    }
                    else
                    {
                        log.WarnFormat("OnOperationReturn: Authenticate on Master failed: ReturnCode: {0} ({1}). Disconnecting...", operationResponse.ReturnCode, operationResponse.DebugMessage);
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