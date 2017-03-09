// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConnectionStateBase.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Photon.Stardust.S2S.Server.ConnectionStates
{
    using ExitGames.Logging;

    using Photon.SocketServer;
    using Photon.Stardust.S2S.Server.ClientConnections;

    using PhotonHostRuntimeInterfaces;

    public class ConnectionStateBase : IConnectionState
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        
        public virtual void OnOperationReturn(ClientConnection client, OperationResponse operationResponse)
        {
            log.WarnFormat("OnOperationReturn: unexpected return code {0} of operation {1}", operationResponse.ReturnCode, operationResponse.OperationCode);
        }

        public virtual void OnPeerStatusCallback(ClientConnection client, DisconnectReason returnCode)
        {
            log.WarnFormat(
                "{1}: OnPeerStatusCallback - unexpected return code {0}", returnCode, client.GetHashCode());

            if (log.IsInfoEnabled)
            {
                log.InfoFormat("{0}", returnCode);
            }
        }

        

        public virtual void OnUpdate(ClientConnection client)
        {

        }

        public virtual void StopClient(ClientConnection client)
        {

        }

        public virtual void EnterState(ClientConnection client)
        {

        }

        public virtual void TransitState(ClientConnection client)
        {

        }
    }
}
