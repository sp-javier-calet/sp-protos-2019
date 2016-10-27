using System.Text;
using System.Collections.Generic;
using SocialPoint.AdminPanel;
using SocialPoint.Base;
using UnityEngine.UI;

namespace SocialPoint.Social
{
    public class AdminPanelSocialFramework : IAdminPanelConfigurer, IAdminPanelManagedGUI
    {
        readonly ConnectionManager _connection;
        readonly ChatManager _chat;
        AdminPanelLayout _layout;
        AdminPanelConsole _console;

        AdminPanelSocialFrameworkUser _userPanel;
        AdminPanelSocialFrameworkChat _chatPanel;

        public AdminPanelSocialFramework(ConnectionManager connection, ChatManager chat)
        {
            _connection = connection;
            _chat = chat;
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            _console = adminPanel.Console;
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Social Framework", this));

            // Cache nested panel
            _userPanel = new AdminPanelSocialFrameworkUser(_connection);
            _chatPanel = new AdminPanelSocialFrameworkChat(_chat);
        }

        public void OnOpened()
        {
            _connection.OnConnected += OnConnected;
            _connection.OnError += OnError;
            _connection.OnClosed += OnDisconnected;
        }

        public void OnClosed()
        {
            _connection.OnConnected -= OnConnected;
            _connection.OnError -= OnError;
            _connection.OnClosed -= OnDisconnected;
            _layout = null;
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            _layout = layout;

            layout.CreateLabel("Social Framework");
            layout.CreateMargin();

            var connectLabel = _connection.IsConnecting ? "Connecting..." : "Connect";
            layout.CreateToggleButton(connectLabel, _connection.IsConnected, value => {
                // Abort connection
                if(_connection.IsConnecting)
                {
                    _connection.Disconnect();
                }
                else if(value)
                {
                    _connection.Connect();
                }
                else
                {
                    _connection.Disconnect();
                }
                layout.Refresh();
            });
            layout.CreateOpenPanelButton("User", _userPanel, !_connection.IsConnected);
            layout.CreateToggleButton("Debug Mode", _connection.DebugEnabled, value => {
                _connection.DebugEnabled = value;
            });
            layout.CreateMargin();

            layout.CreateOpenPanelButton("Chat", _chatPanel, _chat != null && _connection.IsConnected);
        }

        void OnConnected()
        {
            _console.Print("Social Framework client connected");
            _layout.Refresh();
        }

        void OnError(Error err)
        {
            _console.Print("Social Framework client error: " + err);
            _layout.Refresh();
        }

        void OnDisconnected()
        {
            _console.Print("Social Framework client disconnected");
            _layout.Refresh();
        }

        #region Chat

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
                    layout.CreateOpenPanelButton(room.Type, new AdminPanelSocialFrameworkChatRoom(room), room.Subscribed);
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

        #endregion

        #region Chat Rooms

        class AdminPanelSocialFrameworkUser : IAdminPanelGUI
        {
            readonly ConnectionManager _connection;
            ConnectionManager.UserData _selected;

            Dictionary<string, ConnectionManager.UserData> _users = new Dictionary<string, ConnectionManager.UserData> {
                { "Current User", default(ConnectionManager.UserData) },
                { "LoD User 1", new ConnectionManager.UserData(200001L, "18094023679616948036931678079514") },
                { "LoD User 2", new ConnectionManager.UserData(200002L, "18094023679616948036931678079514") }
            };

            public AdminPanelSocialFrameworkUser(ConnectionManager connection)
            {
                _connection = connection;
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("SP-Rocket User");
                layout.CreateMargin();

                var itr = _users.GetEnumerator();
                while(itr.MoveNext())
                {
                    var entry = itr.Current;
                    CreateUserSelector(layout, entry.Key, entry.Value);
                }
                itr.Dispose();

                CreateUserInfo(layout, _selected);
            }

            void CreateUserSelector(AdminPanelLayout layout, string label, ConnectionManager.UserData user)
            {
                layout.CreateToggleButton(label, user == _selected, value => {
                    _selected = user;
                    _connection.ForcedUser = _selected;
                    layout.Refresh();
                });
            }

            void CreateUserInfo(AdminPanelLayout layout, ConnectionManager.UserData user)
            {
                var content = new StringBuilder();

                if(user != null)
                {
                    content.AppendLine("UserId" + user.UserId);
                }
                else
                {
                    content.AppendLine("Current logged user");
                }

                layout.CreateVerticalScrollLayout().CreateTextArea(content.ToString());
            }
        }

        #endregion

        #region Chat Rooms

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

        #endregion
    }
}