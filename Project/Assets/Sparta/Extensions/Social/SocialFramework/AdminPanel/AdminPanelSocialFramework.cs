using System;
using System.Text;
using System.Collections.Generic;
using SocialPoint.AdminPanel;
using SocialPoint.Base;
using SocialPoint.WAMP;

namespace SocialPoint.Social
{
    public class AdminPanelSocialFramework : IAdminPanelConfigurer, IAdminPanelManagedGUI
    {
        readonly ConnectionManager _connection;
        readonly ChatManager _chat;
        readonly AlliancesManager _alliances;
        readonly PlayersManager _playersManager;
        readonly SocialManager _socialManager;
        readonly StringBuilder _content;

        AdminPanelLayout _layout;
        AdminPanelConsole _console;

        AdminPanelSocialFrameworkUser _userPanel;
        AdminPanelSocialFrameworkChat _chatPanel;
        AdminPanelSocialFrameworkAlliances _alliancesPanel;
        AdminPanelSocialFrameworkPlayers _playersPanel;

        public AdminPanelSocialFramework(ConnectionManager connection, ChatManager chat, AlliancesManager alliances, PlayersManager playersManager, SocialManager socialManager)
        {
            _connection = connection;
            _chat = chat;
            _alliances = alliances;
            _playersManager = playersManager;
            _socialManager = socialManager;
            _content = new StringBuilder();
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            _console = adminPanel.Console;
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Social Framework", this));

            // Cache nested panel
            _userPanel = new AdminPanelSocialFrameworkUser(_connection);
            _chatPanel = new AdminPanelSocialFrameworkChat(_chat, _console);
            _alliancesPanel = new AdminPanelSocialFrameworkAlliances(_alliances, _playersManager, _socialManager, _console);
            _playersPanel = new AdminPanelSocialFrameworkPlayers(_playersManager, _socialManager, _console);
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

            layout.CreateOpenPanelButton("Players", _playersPanel, _playersPanel != null && connected);
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

        #region Base Panels

        /// <summary>
        /// Base panel with connection management
        /// </summary>
        public abstract class BaseRequestPanel : IAdminPanelManagedGUI
        {
            protected WAMPRequest _wampRequest;
            protected Error _wampRequestError;

            public BaseRequestPanel()
            {
            }

            protected void Cancel()
            {
                if(_wampRequest != null)
                {
                    _wampRequest.Dispose();
                }
                _wampRequest = null;
                _wampRequestError = null;
            }

            public virtual void OnOpened()
            {
                Cancel();
            }

            public virtual void OnClosed()
            {
                Cancel();
            }

            public abstract void OnCreateGUI(AdminPanelLayout layout);
        }

        public class BaseUserInfoPanel : BaseRequestPanel
        {
            protected SocialPlayer _member;
            protected readonly PlayersManager _playersManager;
            protected readonly AdminPanelConsole _console;

            public string UserId;

            public BaseUserInfoPanel(PlayersManager playersManager, AdminPanelConsole console)
            {
                _playersManager = playersManager;
                _console = console;
            }

            public override void OnOpened()
            {
                base.OnOpened();
                _member = null;
            }

            void OnInfoLoaded(AdminPanelLayout layout)
            {

            }

            public override void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("User Info");
                layout.CreateMargin();

                if(_member != null)
                {
                    layout.CreateVerticalLayout().CreateTextArea(_member.ToString());
                    layout.CreateMargin();

                    OnInfoLoaded(layout);
                }
                else
                {
                    if(_wampRequest == null)
                    {
                        _wampRequest = _playersManager.LoadUserInfo(UserId, null,
                            (err, member) => {
                                if(Error.IsNullOrEmpty(err))
                                {
                                    _member = member;
                                    _console.Print(string.Format("User {0} loaded successfully", member.Uid));
                                    Cancel();
                                    layout.Refresh();
                                }
                                else
                                {
                                    _console.Print(string.Format("Error loading user: {0} ", err));
                                    _wampRequestError = err;
                                    layout.Refresh();
                                }
                            });
                    } 
                    if(Error.IsNullOrEmpty(_wampRequestError))
                    {
                        layout.CreateLabel(string.Format("Loading user {0}...", UserId));
                    }
                    else
                    {
                        layout.CreateLabel("Load user request failed");
                        layout.CreateTextArea(_wampRequestError.ToString());
                        layout.CreateButton("Retry", () => {
                            Cancel();
                            layout.Refresh();
                        });

                        layout.CreateMargin();
                    }
                }
            }
        }

        #endregion

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