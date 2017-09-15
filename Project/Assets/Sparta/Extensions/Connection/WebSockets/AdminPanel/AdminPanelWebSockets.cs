#if ADMIN_PANEL 

using UnityEngine.UI;
using System.Text;
using SocialPoint.IO;
using SocialPoint.AdminPanel;
using SocialPoint.Network;
using SocialPoint.Utils;

namespace SocialPoint.WebSockets
{
    public class AdminPanelWebSockets : IAdminPanelConfigurer, IAdminPanelGUI, IUpdateable, INetworkClientDelegate, INetworkMessageReceiver
    {
        const long AutoSendInterval = 3;

        readonly string _name;
        readonly IWebSocketClient _socket;
        readonly StringBuilder _content;
        Text _text;
        bool _autoSend;
        long _lastAutoSend;

        public AdminPanelWebSockets(IWebSocketClient client, string name)
        {
            _content = new StringBuilder();
            _name = name;
            _socket = client;
            _socket.AddDelegate(this);
            _socket.RegisterReceiver(this);
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Websockets", new AdminPanelNestedGUI(_name, this)));
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel(_socket.GetType().Name);

            layout.CreateToggleButton("Connect", _socket.Connected, (value) => {
                if(value)
                {
                    _socket.Connect();
                }
                else
                {
                    _socket.Disconnect();
                }
                layout.Refresh();
            });

            layout.CreateTextInput(value => 
                {
                    _socket.Urls = new string[]{value};
                    layout.Refresh();
                });

            for(int i = 0; i < _socket.Urls.Length; ++i)
            {
                layout.CreateLabel(_socket.Urls[i]);
            }
            layout.CreateButton("Ping", _socket.Ping);

            layout.CreateButton("Send", SendDefaultMessage, _socket.Connected);

            layout.CreateToggleButton("Send periodically", _autoSend, value => 
                {
                    _autoSend = value;
                    if(value)
                    {
                        layout.RegisterUpdateable(this);
                    }
                    else
                    {
                        layout.UnregisterUpdateable(this);
                    }
                });

            _text = layout.CreateVerticalScrollLayout().CreateTextArea(_content.ToString());
        }

        public void Update()
        {
            var now = TimeUtils.Timestamp;
            if(now - _lastAutoSend > AutoSendInterval)
            {
                SendDefaultMessage();
                _lastAutoSend = now;
            }
        }

        void RefreshText()
        {
            if(_text != null)
            {
                _text.text = _content.ToString();
            }
        }

        void SendDefaultMessage()
        {
            var data = new NetworkMessageData();
            var msg = _socket.CreateMessage(data);
            msg.Writer.Write("hello");
            msg.Send();
        }

        #region INetworkClientDelegate implementation

        public void OnClientConnected()
        {
            _content.AppendLine("Connected to " + _socket.ConnectedUrl);
            RefreshText();
        }

        public void OnClientDisconnected()
        {
            _content.AppendLine("Disconnected from " + _socket.ConnectedUrl);
            RefreshText();
        }

        public void OnMessageReceived(NetworkMessageData data)
        {
            _content.AppendLine("Message received");
            RefreshText();
        }

        public void OnNetworkError(SocialPoint.Base.Error err)
        {
            _content.AppendLine("Network error: " + err.Msg);
            RefreshText();
        }

        #endregion

        #region INetworkMessageReceiver implementation

        public void OnMessageReceived(NetworkMessageData data, IReader reader)
        {
            _content.AppendLine("Message: " + reader.ReadString());
            RefreshText();
        }

        #endregion
    }
}

#endif
