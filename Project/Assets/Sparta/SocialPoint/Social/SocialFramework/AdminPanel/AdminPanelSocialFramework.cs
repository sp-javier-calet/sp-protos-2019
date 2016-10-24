using System.Text;
using System.Collections.Generic;
using SocialPoint.AdminPanel;
using SocialPoint.Network;
using UnityEngine.UI;

namespace SocialPoint.Social
{
    public class AdminPanelSocialFramework : IAdminPanelConfigurer, IAdminPanelGUI
    {
        readonly INetworkClient _client;
        readonly ConnectionManager _connection;
        readonly ChatManager _chat;

        public AdminPanelSocialFramework(INetworkClient client, ConnectionManager connection, ChatManager chat)
        {
            _client = client;
            _connection = connection;
            _chat = chat;
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Social Framework", this));
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Social Framework");
            layout.CreateMargin();

            layout.CreateOpenPanelButton("Network client", new AdminPanelSocialFrameworkNetworkClient(_client));

            if(_connection.IsConnected)
            {
                if(_chat != null)
                {
                    layout.CreateOpenPanelButton("Chat", new AdminPanelSocialFrameworkChat(_chat));
                }
            }
            else
            {
                layout.CreateLabel("Connection Manager is disconnected");
            }

            layout.CreateToggleButton("Debug Mode", _connection.DebugEnabled, value => {
                _connection.DebugEnabled = value;
            });
        }

        class AdminPanelSocialFrameworkNetworkClient : IAdminPanelManagedGUI, INetworkClientDelegate
        {
            Text _text;
            readonly INetworkClient _client;
            readonly StringBuilder _content;

            public AdminPanelSocialFrameworkNetworkClient(INetworkClient client)
            {
                _client = client;
                _content = new StringBuilder();;
            }

            public void OnOpened()
            {
                _client.AddDelegate(this);
            }

            public void OnClosed()
            {
                _client.RemoveDelegate(this);
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Social Framework Network");
                layout.CreateToggleButton("Connect", _client.Connected, value =>
                    {
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
                _text = layout.CreateVerticalScrollLayout().CreateTextArea(_content.ToString());
            }

            void RefreshLog()
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
                RefreshLog();
            }

            public void OnClientDisconnected()
            {
                _content.AppendLine("Disconnected");
                RefreshLog();
            }

            public void OnMessageReceived(NetworkMessageData data)
            {
                _content.AppendLine("Message received from " + data.ClientId);
                RefreshLog();
            }

            public void OnNetworkError(SocialPoint.Base.Error err)
            {
                _content.AppendLine("Network error: " + err);
                RefreshLog();
            }

            #endregion
        }

        class AdminPanelSocialFrameworkChat : IAdminPanelGUI
        {
            readonly ChatManager _chat;

            public AdminPanelSocialFrameworkChat(ChatManager chat)
            {
                _chat = chat;
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Chat");
                layout.CreateMargin();

                var itr = _chat.GetRooms();
                while(itr.MoveNext())
                {
                    var room = itr.Current;
                    layout.CreateOpenPanelButton(room.Type, new AdminPanelSocialFrameworkChatRoom(room));
                }
                itr.Dispose();

                layout.CreateMargin();
                layout.CreateLabel("Ban info:");
                layout.CreateLabel(GetBanInfo());
                layout.CreateMargin();
            }

            string GetBanInfo()
            {
                return _chat.ChatBanEndTimestamp > 0 ? 
                    string.Format("You are banned until {0}", _chat.ChatBanEndTimestamp) : 
                    "You are a legal player!";
            }
        }

        class AdminPanelSocialFrameworkChatRoom : IAdminPanelManagedGUI
        {
            public string Name
            {
                get
                {
                    return _room.Type;
                }
            }

            Text _text;
            readonly StringBuilder _content;
            readonly IChatRoom _room;

            public AdminPanelSocialFrameworkChatRoom(IChatRoom room)
            {
                _room = room;
                _content = new StringBuilder();
            }

            public void OnOpened()
            {
                _room.Messages.OnMessageAdded += OnMessageListChanged;
                _room.Messages.OnMessageEdited += OnMessageListChanged;
            }

            public void OnClosed()
            {
                _room.Messages.OnMessageAdded -= OnMessageListChanged;
                _room.Messages.OnMessageEdited -= OnMessageListChanged;
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel(Name);
                layout.CreateMargin();

                RefreshChatContent();

                _text = layout.CreateTextArea(_content.ToString());
                layout.CreateTextInput(_room.SendDebugMessage);
            }

            void OnMessageListChanged(int idx)
            {
                RefreshChatContent();
            }

            void RefreshChatContent()
            {
                _content.Length = 0;

                _room.Messages.ProcessMessages((idx, msg) => {
                    if(msg.IsWarning)
                    {
                        _content.AppendLine(msg.Text);
                    }
                    else
                    {
                        if(msg.IsSending)
                        {
                            _content.Append(">>");
                        }

                        var allianceText = string.Empty;
                        if(msg.HasAlliance)
                        {
                            allianceText = string.Format("[{0}({1})]", msg.AllianceName, msg.AllianceId);
                        }
                        _content.AppendFormat("{0}({1}) {2}: {3}", msg.PlayerName, msg.PlayerId, allianceText, msg.Text).AppendLine();
                    }
                });

                if(_text != null)
                {
                    _text.text = _content.ToString();
                }
            }
        }
    }
}