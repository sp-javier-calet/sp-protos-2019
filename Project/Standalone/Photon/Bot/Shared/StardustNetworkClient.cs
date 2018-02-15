using Photon.SocketServer;
using Photon.Stardust.S2S.Server;
using Photon.Stardust.S2S.Server.ClientConnections;
using Photon.Stardust.S2S.Server.Enums;
using PhotonHostRuntimeInterfaces;
using System.IO;
using System.Collections.Generic;
using SocialPoint.IO;
using SocialPoint.Base;

namespace SocialPoint.Network
{
    public class StardustNetworkClient : GamingPeer, INetworkClient
    {
        ClientConnection _clientConnection;
        public bool SendReliable = true;
        public bool UseEncryption = false;
        public byte ChannelId = 0;

        INetworkMessageReceiver _receiver;
        List<INetworkClientDelegate> _delegates = new List<INetworkClientDelegate>();

        public StardustNetworkClient(ClientConnection clientConnection, ApplicationBase application) : base(clientConnection, application)
        {
            _clientConnection = clientConnection;
        }

        public byte ClientId
        {
            get
            {
                return (byte)_clientConnection.Number;
            }
        }

        public void AddDelegate(INetworkClientDelegate dlg)
        {
            _delegates.Add(dlg);
        }

        public void Connect()
        {
            _clientConnection.ConnectToServer();
        }

        public INetworkMessage CreateMessage(NetworkMessageData data)
        {
            return new StardustNetworkMessage(this, data, Send);
        }

        public int GetDelay(int networkTimestamp)
        {
            return 0;
        }

        public bool LatencySupported
        {
            get
            {
                return false;
            }
        }

        public int Latency
        {
            get
            {
                return 0;
            }
        }

        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        public void RemoveDelegate(INetworkClientDelegate dlg)
        {
            _delegates.Remove(dlg);
        }

        public void Send(byte code, byte[] data)
        {
            var encodedData = HttpEncoding.Encode(data, HttpEncoding.LZ4);
            var wrap = new Dictionary<byte, object> { { LiteOpKey.Data, encodedData }, { LiteOpKey.Code, code } };
            _clientConnection.Peer.SendOperationRequest(new OperationRequest(LiteOpCode.RaiseEvent, wrap), new SendParameters() { Unreliable = !SendReliable, Encrypted = UseEncryption, ChannelId = ChannelId });
        }

        protected override void OnEvent(IEventData eventData, SendParameters sendParameters)
        {
            base.OnEvent(eventData, sendParameters);

            if(eventData.Parameters.ContainsKey(LiteOpKey.Data))
            {
                var data = eventData.Parameters[LiteOpKey.Data] as byte[];
                if (data != null && _receiver != null)
                {
                    var decoded = HttpEncoding.Decode(data, HttpEncoding.LZ4);
                    var stream = new MemoryStream(decoded);
                    _receiver.OnMessageReceived(new NetworkMessageData { MessageType = eventData.Code }, new SystemBinaryReader(stream));
                }
            }
        }

        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            base.OnDisconnect(reasonCode, reasonDetail);
            for(int i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientDisconnected();
            }
        }

        protected override void OnConnectionEstablished(object responseObject)
        {
            base.OnConnectionEstablished(responseObject);
            for(int i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientConnected();
            }
        }

        protected override void OnConnectionFailed(int errorCode, string errorMessage)
        {
            base.OnConnectionFailed(errorCode, errorMessage);
            for(int i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnNetworkError(new Error(errorCode, errorMessage));
            }
        }
    }
}
