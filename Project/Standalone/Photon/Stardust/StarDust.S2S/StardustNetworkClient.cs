using Photon.SocketServer;
using Photon.Stardust.S2S.Server.ClientConnections;
using Photon.Stardust.S2S.Server.Enums;
using SocialPoint.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Photon.Stardust.S2S.Server
{
    class StardustNetworkClient : INetworkClient
    {
        ClientConnection _clientConnection;
        public bool SendReliable = true;
        public bool UseEncryption = false;
        public byte ChannelId = 0;

        public StardustNetworkClient(ClientConnection clientConnection)
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

        public bool Connected
        {
            get
            {
                return _clientConnection.Peer.Connected;
            }
        }

        public void AddDelegate(INetworkClientDelegate dlg)
        {
            _clientConnection.Peer.AddDelegate(dlg);
        }

        public void Connect()
        {
            _clientConnection.ConnectToServer();

        }

        public INetworkMessage CreateMessage(NetworkMessageData data)
        {
            return new PeerMessage(_clientConnection.Peer, data, Send);
        }

        public void Disconnect()
        {
            _clientConnection.Stop();
        }

        public int GetDelay(int networkTimestamp)
        {
            return 0;
        }

        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            _clientConnection.Peer.RegisterReceiver(receiver);
        }

        public void RemoveDelegate(INetworkClientDelegate dlg)
        {
            _clientConnection.Peer.RemoveDelegate(dlg);
        }

        public void Send(byte code, byte[] data)
        {
            var wrap = new Dictionary<byte, object> { { LiteOpKey.Data, data }, { LiteOpKey.Code, code } };
            _clientConnection.Peer.SendOperationRequest(new OperationRequest(LiteOpCode.RaiseEvent, wrap), new SendParameters() { Unreliable = !SendReliable, Encrypted = UseEncryption, ChannelId = ChannelId });
        }
    }
}
