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
    using SocialPoint.Network;
    using System.Collections.Generic;
    using SocialPoint.Base;
    using System.IO;
    using Enums;
    using SocialPoint.IO;

    public class GamingPeer : OutboundS2SPeer
    {
        private readonly ClientConnection client; 
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        INetworkMessageReceiver _receiver;
        List<INetworkClientDelegate> _delegates = new List<INetworkClientDelegate>();

        public GamingPeer(ClientConnection clientConnection, ApplicationBase application)
            : base(application)
        {
            this.client = clientConnection; 
        }

        protected override void OnConnectionEstablished(object responseObject)
        {
            this.client.OnConnected(this);
            for (int i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientConnected();
            }
        }

        protected override void OnConnectionFailed(int errorCode, string errorMessage)
        {
            log.ErrorFormat("Error establishing connection: {0} {1}", errorCode, errorMessage);
            for (int i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnNetworkError(new Error(errorCode, errorMessage));
            }
        }

        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            if (log.IsInfoEnabled)
            {
                log.InfoFormat("{0} disconnected: {1}", this.ConnectionId, reasonCode);
            }
            
           this.client.State.OnPeerStatusCallback(this.client, reasonCode);
            for (int i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientDisconnected();
            }
        }

        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            throw new System.NotImplementedException();
        }

        protected override void OnEvent(IEventData eventData, SendParameters sendParameters)
        {
            this.client.OnEvent(eventData);

            if (eventData.Parameters.ContainsKey(LiteOpKey.Data))
            {
                var stream = new MemoryStream((byte[])eventData.Parameters[LiteOpKey.Data]);
                if (_receiver != null)
                {
                    _receiver.OnMessageReceived(new NetworkMessageData { MessageType = eventData.Code }, new SystemBinaryReader(stream));
                }
            }
        }

        protected override void OnOperationResponse(OperationResponse operationResponse, SendParameters sendParameters)
        {
            this.client.OnOperationResponse(operationResponse);   
        }

        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        public void AddDelegate(INetworkClientDelegate dlg)
        {
            _delegates.Add(dlg);
        }

        public void RemoveDelegate(INetworkClientDelegate dlg)
        {
            _delegates.Remove(dlg);
        }
    }
}
