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

    internal class Connected : ConnectionStateBase
    {
        public static readonly Connected Instance = new Connected();

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        #region Implemented Interfaces

        #region IConnectionState

        public override void EnterState(ClientConnection client)
        {
             // join: 
            this.OpJoin(client);

            Counters.ReliableOperationsSent.Increment();
            WindowsCounters.ReliableOperationsSent.Increment();
        }
     
        public override void StopClient(ClientConnection client)
        {
            Counters.ConnectedClients.Decrement();
            WindowsCounters.ConnectedClients.Decrement();

            client.State = Disconnected.Instance; 
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

        public override void OnPeerStatusCallback(ClientConnection client, DisconnectReason returnCode)
        {
            if (log.IsInfoEnabled)
            {
                log.InfoFormat("{0}", returnCode);
            }

            Counters.ConnectedClients.Decrement();
            WindowsCounters.ConnectedClients.Decrement();

            client.State = Disconnected.Instance;
            client.OnDisconnected();
        }

        public override void OnOperationReturn(ClientConnection client, OperationResponse operationResponse)
        {
            Counters.ReceivedOperationResponse.Increment();
            WindowsCounters.ReceivedOperationResponse.Increment();

            switch (operationResponse.OperationCode)
            {
                case LiteOpCode.Join:
                    {
                        if (operationResponse.ReturnCode == 0)
                        {
                            this.TransitState(client);
                        }
                        else
                        {
                            log.WarnFormat("OnOperationReturn: unexpected return code {0} of operation {1}", operationResponse.ReturnCode, operationResponse.OperationCode);
                            client.Stop();
                        }

                        break;
                    }

                default:
                    {
                        log.WarnFormat("OnOperationReturn: unexpected return code {0} of operation {1}", operationResponse.ReturnCode, operationResponse.OperationCode);
                        break;
                    }
            }
        }

        public override void OnUpdate(ClientConnection client, int elapsedMiliSeconds)
        {
            //client.PeerService();
            client.EnqueueUpdate();
        }


        private void OpJoin(ClientConnection client)
        {
            var wrap = new Dictionary<byte, object> { { LiteOpKey.GameId, client.GameName } };
            client.Peer.SendOperationRequest(new OperationRequest(LiteOpCode.Join, wrap), new SendParameters() { Unreliable = false, Encrypted = Settings.UseEncryption}); 
        }

        

        #endregion

        #endregion
    }
}