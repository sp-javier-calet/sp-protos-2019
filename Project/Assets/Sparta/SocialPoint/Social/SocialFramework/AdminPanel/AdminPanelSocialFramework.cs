using System.Text;
using System.Collections.Generic;
using SocialPoint.AdminPanel;
using SocialPoint.Network;
using UnityEngine.UI;

namespace SocialPoint.Social
{
    public class AdminPanelSocialFramework : IAdminPanelConfigurer, IAdminPanelGUI
    {
        readonly ConnectionManager _connection;
        readonly ChatManager _chat;

        public AdminPanelSocialFramework(ConnectionManager connection, ChatManager chat)
        {
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

            layout.CreateToggleButton("Connect", _connection.IsConnected, value => {
                if(value)
                {
                    _connection.Connect();
                }
                else
                {
                    _connection.Disconnect();
                }
                layout.Refresh();
            });
            layout.CreateButton("Abort", () => {
                _connection.Disconnect();
                layout.Refresh();
            }, _connection.IsConnecting);

            layout.CreateOpenPanelButton("Chat", new AdminPanelSocialFrameworkChat(_chat), _chat != null && _connection.IsConnected);

            layout.CreateToggleButton("Debug Mode", _connection.DebugEnabled, value => {
                _connection.DebugEnabled = value;
            });
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