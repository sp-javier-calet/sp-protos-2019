using UnityEngine.UI;
using System.Text;
using SocialPoint.IO;
using SocialPoint.AdminPanel;
using SocialPoint.Network;

namespace SocialPoint.WebSockets
{
    public class AdminPanelWebSockets : IAdminPanelConfigurer, IAdminPanelGUI, INetworkClientDelegate, INetworkMessageReceiver
    {
        readonly INetworkClient _client;
        readonly StringBuilder _content;
        Text _text;

        public AdminPanelWebSockets(INetworkClient client)
        {
            _content = new StringBuilder();

            _client = client;
            _client.AddDelegate(this);
            _client.RegisterReceiver(this);
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Websockets", this));
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateToggleButton("Connect", _client.Connected, (value) => {
                if(value)
                {
                    _client.Connect();
                }
                else
                {
                    _client.Disconnect();
                }
                layout.Refresh();
            });

            layout.CreateButton("Send", () => {
                var data = new NetworkMessageData();
                var msg = _client.CreateMessage(data);
                msg.Writer.Write("hello");
                msg.Send();
            }, _client.Connected);

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
            _content.AppendLine("Connected");
            RefreshText();
        }

        public void OnClientDisconnected()
        {
            _content.AppendLine("Disconnected");
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
