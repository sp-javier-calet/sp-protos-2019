// -----------------------------------------------------------------------
// <copyright file="GamingPeer.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Photon.Stardust.S2S.Server
{
    using System;

    using ExitGames.Logging;

    using Photon.SocketServer;
    using Photon.SocketServer.ServerToServer;
    using Photon.Stardust.S2S.Server.ClientConnections;

    using PhotonHostRuntimeInterfaces;

    public class GamingPeer : OutboundS2SPeer
    {
        private readonly ClientConnection client; 
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public GamingPeer(ClientConnection clientConnection, ApplicationBase application)
            : base(application)
        {
            this.client = clientConnection; 
        }

        protected override void OnConnectionEstablished(object responseObject)
        {
            this.client.OnConnected(this);
        }

        protected override void OnConnectionFailed(int errorCode, string errorMessage)
        {
            log.ErrorFormat("Error establishing connection: {0} {1}", errorCode, errorMessage);
        }

        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            if (log.IsInfoEnabled)
            {
                log.InfoFormat("{0} disconnected: {1}", this.ConnectionId, reasonCode);
            }
            
           this.client.State.OnPeerStatusCallback(this.client, reasonCode);
        }

        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            throw new System.NotImplementedException();
        }

        protected override void OnEvent(IEventData eventData, SendParameters sendParameters)
        {
            this.client.OnEvent(eventData);
        }

        protected override void OnOperationResponse(OperationResponse operationResponse, SendParameters sendParameters)
        {
            this.client.OnOperationResponse(operationResponse);   
        }
    }
}
