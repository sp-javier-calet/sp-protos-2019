//-----------------------------------------------------------------------
// NetworkServerMatchHandler.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using SocialPoint.Base;
using SocialPoint.Network;

namespace SocialPoint.Examples.Sockets
{
    class NetworkServerMatchHandler : INetworkServerDelegate, INetworkMessageReceiver
    {
        string _matchId;

        INetworkMessageSender _sender;

        public NetworkServerMatchHandler(string matchId, INetworkMessageSender sender)
        {
            _matchId = matchId;
            _sender = sender;
        }

        #region INetworkServerDelegate implementation

        void INetworkServerDelegate.OnServerStarted()
        {
            Log.d("NetworkServerMatchHandler OnServerStarted: " + _matchId);
        }

        void INetworkServerDelegate.OnServerStopped()
        {
            Log.d("NetworkServerMatchHandler OnServerStopped:" + _matchId);
        }

        void INetworkServerDelegate.OnClientConnected(byte clientId)
        {
            Log.d("NetworkServerMatchHandler OnClientConnected: ClientID " + clientId + " _matchId: " + _matchId);

            _sender.SendMessage(new NetworkMessageData
            {
                MessageType = TypeMessages.ConnectMessageType,
            }, new MatchConnectMessage(_matchId));

        }

        void INetworkServerDelegate.OnClientDisconnected(byte clientId)
        {
            Log.d("NetworkServerMatchHandler OnClientDisconnected: ClientID " + clientId + " _matchId: " + _matchId);
        }

        void INetworkServerDelegate.OnMessageReceived(NetworkMessageData data)
        {
            Log.d("NetworkServerMatchHandler INetworkServerDelegate OnMessageReceived: ClientID " + data.ClientIds[0] + " _matchId: " + _matchId);
        }

        void INetworkServerDelegate.OnNetworkError(SocialPoint.Base.Error err)
        {
            Log.e("NetworkServerMatchHandler OnNetworkError: " + err.ToString() + " _matchId: " + _matchId);
        }

        #endregion

        #region INetworkMessageReceiver implementation

        public void OnMessageReceived(NetworkMessageData data, SocialPoint.IO.IReader reader)
        {
            if (data.MessageType == TypeMessages.DefaultMessageType)
            {
                var cmd = new DefaultMessage();
                cmd.Deserialize(reader);

                _sender.SendMessage(new NetworkMessageData
                {
                    MessageType = TypeMessages.DefaultMessageType,
                }, cmd);

                Log.d("NetworkServerMatchHandler INetworkMessageReceiver OnMessageReceived: ClientID " + data.ClientIds[0] + " _matchId: " + _matchId + " Message: " + cmd.Message);
            }
        }
        #endregion
    }
}