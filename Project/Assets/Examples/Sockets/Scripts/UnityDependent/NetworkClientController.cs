using System;
using SocialPoint.Base;
using SocialPoint.Dependency;
using SocialPoint.Network;
using SocialPoint.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.Sockets
{

    public class NetworkClientController : MonoBehaviour, INetworkClientDelegate, INetworkMessageReceiver
    {
        enum Protocol
        {
            TCP = 0,
            UDP = 1
        }

        [SerializeField]
        InputField _matchInputField;

        [SerializeField]
        InputField _ipInputField;

        [SerializeField]
        InputField _portInputField;

        [SerializeField]
        InputField _messageInputField;

        [SerializeField]
        Text _logText;

        [SerializeField]
        Dropdown _protocolDropdown;

        INetworkClient _netClient;
        string _matchId;
        string _ipAdrress;
        int _port;

        Protocol _protocol = Protocol.TCP;

        public void CreateOrJoinMatch()
        {
            _matchId = _matchInputField.text;
            _ipAdrress = _ipInputField.text;
            _port = int.Parse(_portInputField.text);


            PrintLog("Session Name: " + _matchInputField.text);
            PrintLog("IP Adress: " + _ipInputField.text);
            PrintLog("Port: " + _portInputField.text);

            //CONNECT CLIENT TO SERVER
            ConnectClient(_ipAdrress,_port);
        }

        void ConnectClient(string ipAdress, int port)
        {
            _protocol = (Protocol)Enum.Parse(typeof(Protocol), _protocolDropdown.value.ToString());

            IUpdateScheduler updater= Services.Instance.Resolve<IUpdateScheduler>();
            switch (_protocol)
            {
                case Protocol.TCP:
                    PrintLog("TCP CLIENT: ");
                    _netClient = new TcpSocketNetworkClient(updater, ipAdress, port); 
                    break;
                case Protocol.UDP:
                    PrintLog("UDP CLIENT: ");
                    _netClient = new UdpSocketNetworkClient(updater);
                    (_netClient as UdpSocketNetworkClient).ServerAddress = ipAdress;
                    (_netClient as UdpSocketNetworkClient).ServerPort = port;
                    break;
            }

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
