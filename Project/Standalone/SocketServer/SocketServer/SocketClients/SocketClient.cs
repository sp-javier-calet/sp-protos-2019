using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.IO;
using SocialPoint.Network;
using SocialPoint.Utils;

namespace SocialPoint.Examples.Sockets
{
    public class SocketClient : INetworkClientDelegate, INetworkMessageReceiver
    {
        public enum Protocol
        {
            TCP,
            UDP
        }

        const string ConnectionKey = "TestConnectionKey";
        const int UpdateTime = 10;
        string _matchId;

        INetworkClient _netClient;

        public SocketClient(Protocol protocol, string ipAdress, int port, string matchID, IUpdateScheduler updateScheduler)
        {
            _matchId = matchID;
            switch (protocol)
            {
                case Protocol.TCP:
                    Log.d("INSTANTIATE TCP CLIENTS: " + " PORT: " + port + " IPAdress: " + ipAdress + " MatchID: "+matchID);

                    _netClient = new TcpSocketNetworkClient(updateScheduler, ipAdress, port);
                    break;
                case Protocol.UDP:
                    Log.d("INSTANTIATE UDP CLIENTS: " + " PORT: " + port + " IPAdress: " + ipAdress+ " MatchID: " + matchID);

                    _netClient = new UdpSocketNetworkClient(updateScheduler, ConnectionKey, UpdateTime);
                    (_netClient as UdpSocketNetworkClient).ServerPort = port;
                    (_netClient as UdpSocketNetworkClient).ServerAddress = ipAdress;
                    break;
            }

            _netClient.AddDelegate(this);
            _netClient.RegisterReceiver(this);
        }

        internal void Connect()
        {
            Log.d("CONNECT CLIENT ");
            _netClient.Connect();
        }

        internal void Disconnect()
        {
            Log.d("DISCONNECT CLIENT ");
            _netClient.Disconnect();

            _netClient.RemoveDelegate(this);
            _netClient.RegisterReceiver(null);
        }
        public void SendMessage(string message)
        {
            Log.d("SEND MESSAGE TO SERVER: " + message);

            _netClient.SendMessage(new NetworkMessageData
            {
                MessageType = TypeMessages.DefaultMessageType,
            }, new DefaultMessage(message));
        }

        void SendMessage(DefaultMessage message)
        {
            Log.d("SEND MESSAGE TO SERVER: " + message);

            _netClient.SendMessage(new NetworkMessageData
            {
                MessageType = TypeMessages.DefaultMessageType,
            }, message);
        }
        void INetworkClientDelegate.OnClientConnected()
        {
            Log.d("CONNECTED CLIENT ");
            _netClient.SendMessage(new NetworkMessageData
            {
                MessageType = TypeMessages.ConnectMessageType,
            }, new MatchConnectMessage(_matchId));
        }

        void INetworkClientDelegate.OnClientDisconnected()
        {
            Log.d("DISCONNECTED CLIENT ");
        }

        void INetworkClientDelegate.OnMessageReceived(NetworkMessageData data)
        {
        }

        void INetworkClientDelegate.OnNetworkError(Error err)
        {
            Log.e(err.Msg);
        }

        void INetworkMessageReceiver.OnMessageReceived(NetworkMessageData data, IReader reader)
        {
            switch (data.MessageType)
            {
                case TypeMessages.ConnectMessageType:
                    var cmdConnect = new MatchConnectMessage();
                    cmdConnect.Deserialize(reader);
                    Log.d("OnMessageReceived ConnectMessageType: " + data.MessageType + " MatchID: " + cmdConnect.MatchId);
                    break;

                case TypeMessages.DefaultMessageType:
                    var defaultMessage = new DefaultMessage();
                    defaultMessage.Deserialize(reader);
                    //SendMessage(defaultMessage);
                    Log.d("OnMessageReceived PingPongMessage: " + data.MessageType + "  Message: " + defaultMessage.Message);
                    break;
            }
        }
    }
}
