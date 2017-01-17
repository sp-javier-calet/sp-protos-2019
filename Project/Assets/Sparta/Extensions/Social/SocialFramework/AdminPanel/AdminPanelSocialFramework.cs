using System;
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
        readonly AlliancesManager _alliances;
        readonly SocialManager _socialManager;
        readonly StringBuilder _content;

        AdminPanelLayout _layout;
        AdminPanelConsole _console;

        AdminPanelSocialFrameworkUser _userPanel;
        AdminPanelSocialFrameworkChat _chatPanel;
        AdminPanelSocialFrameworkAlliances _alliancesPanel;

        public AdminPanelSocialFramework(ConnectionManager connection, ChatManager chat, AlliancesManager alliances, SocialManager socialManager)
        {
            _connection = connection;
            _chat = chat;
            _alliances = alliances;
            _socialManager = socialManager;
            _content = new StringBuilder();
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            _console = adminPanel.Console;
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Social Framework", this));

            // Cache nested panel
            _userPanel = new AdminPanelSocialFrameworkUser(_connection);
            _chatPanel = new AdminPanelSocialFrameworkChat(_chat);
            _alliancesPanel = new AdminPanelSocialFrameworkAlliances(_alliances, _socialManager, _console);
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

            var connected = _connection.IsConnected;

            layout.CreateLabel("Social Framework");
            layout.CreateMargin();

            var connectLabel = _connection.IsConnecting ? "Connecting..." : "Connect";
            layout.CreateToggleButton(connectLabel, connected, value => {
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
            var foldoutLayout = layout.CreateFoldoutLayout("Urls");
            _content.Length = 0;

            var connectedUrl = _connection.ConnectedUrl;
            var urls = _connection.Urls;
            for(var i = 0; i < urls.Length; ++i)
            {
                var url = urls[i];
                if(connectedUrl != null && url == connectedUrl)
                {
                    _content.Append(">> ");
                }
                _content.AppendLine(url);
            }

            foldoutLayout.CreateTextArea(_content.ToString());

            layout.CreateOpenPanelButton("User", _userPanel, !connected);
            layout.CreateToggleButton("Debug Mode", _connection.DebugEnabled, value => {
                _connection.DebugEnabled = value;
            });
            layout.CreateMargin();

            layout.CreateOpenPanelButton("Chat", _chatPanel, _chat != null && connected);
            layout.CreateOpenPanelButton("Alliances", _alliancesPanel, _alliances != null && connected);
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


        #region User

        class AdminPanelSocialFrameworkUser : IAdminPanelGUI
        {
            readonly ConnectionManager _connection;
            readonly StringBuilder _content;
            ConnectionManager.UserData _selected;

            Dictionary<string, ConnectionManager.UserData> _users = new Dictionary<string, ConnectionManager.UserData> {
                { "Current User", default(ConnectionManager.UserData) },
                { "LoD User 1", new ConnectionManager.UserData(200001L, "18094023679616948036931678079514") },
                { "LoD User 2", new ConnectionManager.UserData(200002L, "18094023679616948036931678079514") }
            };

            public AdminPanelSocialFrameworkUser(ConnectionManager connection)
            {
                _connection = connection;
                _content = new StringBuilder();
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
                _content.Length = 0;
                if(user != null)
                {
                    _content.AppendFormat("UserId: {0}", user.UserId).AppendLine()
                        .AppendFormat("Security Token: {0}", user.SecurityToken).AppendLine();
                }
                else
                {
                    _content.AppendLine("Current SocialPointLogin user");
                }

                layout.CreateVerticalScrollLayout().CreateTextArea(_content.ToString());
            }
        }

        #endregion
    }
}