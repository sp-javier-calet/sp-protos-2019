using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using SocialPoint.IO;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;
using SocialPoint.Network;

namespace SocialPoint.WebSockets
{
    public class AdminPanelWebSockets : IAdminPanelConfigurer, IAdminPanelGUI, INetworkClientDelegate, INetworkMessageReceiver
    {
        const string WsUrl = "ws://echo.websocket.org";

        INetworkClient _client;
        IDisposable _disposable;
        Text _text;
        readonly ICoroutineRunner _runner;
        readonly StringBuilder _content;

        public AdminPanelWebSockets(ICoroutineRunner runner)
        {
            _runner = runner;
            _content = new StringBuilder();
        }

        void ClearClient()
        {
            if(_client != null)
            {
                _client.RemoveDelegate(this);
                _client.Disconnect();
                _client = null;
            }

            if(_disposable != null)
            {
                _disposable.Dispose();
                _disposable = null;
            }
        }

        void SetClient(INetworkClient client)
        {
            _client = client;
            _disposable = client as IDisposable;
            _client.RegisterReceiver(this);
            _client.AddDelegate(this);
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Websockets", this));
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateConfirmButton("Websocket-sharp", () => {
                ClearClient();
                SetClient(new WebSocketSharpClient(WsUrl, _runner));
                layout.Refresh();
            });

            layout.CreateConfirmButton("Websocket for Unity", () => {
                ClearClient();
                SetClient(new WebSocketUnityClient(WsUrl, _runner));
                layout.Refresh();
            });

            if(_client != null)
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
            }

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
