using System;
using SocialPoint.Base;
using SocialPoint.Dependency;
using SocialPoint.Network;
using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.Sockets
{

    public class NetworkClientController : MonoBehaviour, INetworkClientDelegate, INetworkMessageReceiver
    {

        [SerializeField]
        InputField _matchInputField;

        [SerializeField]
        InputField _ipInputField;

        [SerializeField]
        InputField _messageInputField;

        [SerializeField]
        Text _logText;

        INetworkClient _netClient;
        string _matchId;
        string _ipAdrress;

        public void CreateOrJoinMatch()
        {
            PrintLog("CreateOrJoinMatch ");

            _matchId = _matchInputField.text;
            _ipAdrress = _ipInputField.text;

            PrintLog("Session Name: " + _matchInputField.text);
            PrintLog("IP Adress: " + _ipInputField.text);

            //CONNECT CLIENT TO SERVER
            ConnectClient(_ipAdrress);
        }

        void ConnectClient(string ipAdress)
        {
            _netClient = Services.Instance.Resolve<INetworkClient>();

            _netClient.AddDelegate(this);
            _netClient.RegisterReceiver(this);
            _netClient.Connect();
        }

        void SendMatchConnectMessage()
        {
            _netClient.SendMessage(new NetworkMessageData
            {
                MessageType = TypeMessages.ConnectMessageType,
            }, new MatchConnectMessage(_matchId));
        }

        void OnApplicationQuit()
        {
            DisconnectClient();
        }

        public void DisconnectClient()
        {
            if (_netClient != null)
            {
                PrintLog("Disconnect Client ");
                _netClient.Disconnect();
                _netClient.RemoveDelegate(this);

                var dclient = _netClient as IDisposable;
                if (dclient != null)
                {
                    dclient.Dispose();
                }
            }
        }

        public void SendMessageToServer()
        {
            string message = _messageInputField.text;
            PrintLog("SEND MESSAGE TO SERVER: " + message);

            _netClient.SendMessage(new NetworkMessageData
            {
                MessageType = TypeMessages.DefaultMessageType,
            }, new DefaultMessage(message));
        }

        void PrintLog(string message)
        {
            _logText.text += "CLIENT: " + message + "\n";
            Log.d(message + "\n");
        }


        void INetworkClientDelegate.OnClientConnected()
        {
            PrintLog("OnClientConnected");
            SendMatchConnectMessage();
        }

        void INetworkClientDelegate.OnClientDisconnected()
        {
            PrintLog("OnClientDisconnected");
        }

        void INetworkClientDelegate.OnMessageReceived(NetworkMessageData data)
        {
        }

        void INetworkClientDelegate.OnNetworkError(SocialPoint.Base.Error err)
        {
            PrintLog("OnNetworkError " + err.ToString());
        }

        public void OnMessageReceived(NetworkMessageData data, SocialPoint.IO.IReader reader)
        {
            switch (data.MessageType)
            {
                case TypeMessages.ConnectMessageType:
                    var cmdConnect = new MatchConnectMessage();
                    cmdConnect.Deserialize(reader);
                    PrintLog("OnMessageReceived ConnectMessageType: " + data.MessageType + " MatchID: " + cmdConnect.MatchId);
                    break;

                case TypeMessages.DefaultMessageType:
                    var cmdPingPong = new DefaultMessage();
                    cmdPingPong.Deserialize(reader);

                    PrintLog("OnMessageReceived PingPongMessage: " + data.MessageType + "  MatchID: " + _matchId + "  Message: " + cmdPingPong.Message);
                    break;
            }
        }
    }
}
