// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Playing.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   The playing.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Photon.Stardust.S2S.Server.ConnectionStates.LoadBalancing
{
    using ExitGames.Logging;

    using Photon.SocketServer;
    using Photon.Stardust.S2S.Server.ClientConnections;
    using Photon.Stardust.S2S.Server.Diagnostics;
    using Photon.Stardust.S2S.Server.Enums;

    using PhotonHostRuntimeInterfaces;

    /// <summary>
    ///   The playing.
    /// </summary>
    internal class Playing : ConnectionStateBase
    {
        #region Constants and Fields

        /// <summary>
        ///   The instance.
        /// </summary>
        public static readonly Playing Instance = new Playing();

        /// <summary>
        ///   The logger.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        #endregion

        #region Implemented Interfaces

        #region IConnectionState
     
        public override void StopClient(ClientConnection client)
        {
            Counters.ConnectedClients.Decrement();
            WindowsCounters.ConnectedClients.Decrement();

            client.State = Disconnected.Instance; 
        }

        /// <summary>
        /// The on operation return.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="operationResponse">
        /// The operation Response.
        /// </param>
        public override void OnOperationReturn(ClientConnection client, OperationResponse operationResponse)
        {
            Counters.ReceivedOperationResponse.Increment();
            WindowsCounters.ReceivedOperationResponse.Increment();

            switch (operationResponse.OperationCode)
            {
                case LiteOpCode.RaiseEvent:
                case LiteOpCode.SetProperties:
                    {
                        if (operationResponse.ReturnCode != 0)
                        {
                            log.WarnFormat(
                                "OnOperationReturn: unexpected return code {0} of operation {1}", operationResponse.ReturnCode, operationResponse.OperationCode);
                            client.Stop();
                        }

                        break;
                    }

                case LiteOpCode.Leave:
                    {
                        if (operationResponse.ReturnCode == 0)
                        {
                            if (log.IsDebugEnabled)
                            {
                                log.Debug("Stopped Playing");
                            }

                            client.State = Connected.Instance;
                            client.StopTimers();
                        }
                        else
                        {
                            log.WarnFormat(
                                "OnOperationReturn: unexpected return code {0} of operation {1}", operationResponse.ReturnCode, operationResponse.OperationCode);
                            client.Stop();
                        }

                        break;
                    }

                case LiteOpCode.Join:
                    {
                        // ignore: ReturnIncoming app returns 10x response
                        break;
                    }

                default:
                    {
                        log.WarnFormat(
                            "OnOperationReturn: unexpected return code {0} of operation {1}", operationResponse.ReturnCode, operationResponse.OperationCode);
                        break;
                    }
            }
        }

        /// <summary>
        ///   The on peer status callback.
        /// </summary>
        /// <param name = "client">
        ///   The client.
        /// </param>
        /// <param name = "returnCode">
        ///   The return code.
        /// </param>
        public override void OnPeerStatusCallback(ClientConnection client, DisconnectReason returnCode)
        {

            if ((returnCode == DisconnectReason.ClientDisconnect && log.IsDebugEnabled) || (log.IsInfoEnabled))
            {
                log.InfoFormat("{0}", returnCode);
            }

            Counters.ConnectedClients.Decrement();
            WindowsCounters.ConnectedClients.Decrement();

            client.State = Disconnected.Instance;

            client.OnDisconnected();
        }

        /// <summary>
        ///   The on update.
        /// </summary>
        /// <param name = "client">
        ///   The client.
        /// </param>
        public override void OnUpdate(ClientConnection client)
        {
            // client.PeerService();
            client.EnqueueUpdate();
        }

        #endregion

        #endregion
    }
}