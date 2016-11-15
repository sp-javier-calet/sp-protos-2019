using UnityEngine.UI;
using System.Text;
using SocialPoint.IO;
using SocialPoint.AdminPanel;
using SocialPoint.Network;

namespace SocialPoint.WebSockets
{
    public class AdminPanelWebSockets : IAdminPanelConfigurer, IAdminPanelGUI, INetworkClientDelegate, INetworkMessageReceiver
    {
        readonly string _name;
        readonly IWebSocketClient _socket;
        readonly StringBuilder _content;
        Text _text;

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
                    _socket.Url = value;
                    layout.Refresh();
                });
            
            layout.CreateLabel(_socket.Url);

            layout.CreateButton("Ping", _socket.Ping);

            layout.CreateButton("Send", () => {
                var data = new NetworkMessageData();
                var msg = _socket.CreateMessage(data);
                msg.Writer.Write("hello");
                msg.Send();
            }, _socket.Connected);

            _text = layout.CreateVerticalScrollLayout().CreateTextArea(_content.ToString());
        }

        void RefreshText()
        {
            if(_text != null)
            {
                _text.text = _content.ToString();
            }
        }

        #region INetworkClientDelegate implementation

        public void OnClientConnected()
        {
            _content.AppendLine("Connected to " + _socket.Url);
            RefreshText();
        }

        public void OnClientDisconnected()
        {
            _content.AppendLine("Disconnected from " + _socket.Url);
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
