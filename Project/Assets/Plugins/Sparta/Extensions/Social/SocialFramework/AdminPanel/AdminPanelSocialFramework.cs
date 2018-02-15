#if ADMIN_PANEL 

using System;
using System.Collections.Generic;
using System.Text;
using SocialPoint.AdminPanel;
using SocialPoint.Base;
using SocialPoint.Connection;
using SocialPoint.WAMP;

namespace SocialPoint.Social
{
    public class AdminPanelSocialFramework : IAdminPanelConfigurer, IAdminPanelManagedGUI
    {
        readonly ConnectionManager _connection;
        readonly SocialManager _socialManager;
        readonly StringBuilder _content;
        readonly PlayersManager _playersManager;

        ChatManager _chatManager;
        AlliancesManager _alliancesManager;
        MessagingSystemManager _messagesManager;
        DonationsManager _donationsManager;

        internal ChatManager ChatManager
        {
            get
            {
                return _chatManager; 
            }
            set
            {
                _chatManager = value;
                _chatPanel = _chatManager != null ? new AdminPanelSocialFrameworkChat(ChatManager, _console) : null;
            }
        }

        internal AlliancesManager AlliancesManager
        {
            get
            {
                return _alliancesManager; 
            }
            set
            {
                _alliancesManager = value;
                _alliancesPanel = _alliancesManager != null ? new AdminPanelSocialFrameworkAlliances(AlliancesManager, _playersManager, _socialManager, _console) : null;
            }
        }

        internal MessagingSystemManager MessagesManager
        {
            get
            {
                return _messagesManager; 
            }
            set
            {
                _messagesManager = value;
                _messagesPanel = _messagesManager != null ? new AdminPanelSocialFrameworkMessagingSystem(_console, MessagesManager) : null;
            }
        }

        internal DonationsManager DonationsManager
        {
            get
            {
                return _donationsManager; 
            }
            set
            {
                var localUserId = _connection.LoginData.UserId;
                _donationsManager = value;
                _donationsPanel = _donationsManager != null ? new AdminPanelSocialFrameworkDonations(_console, DonationsManager, (long)localUserId) : null;
            }
        }

        AdminPanelLayout _layout;
        AdminPanelConsole _console;

        AdminPanelSocialFrameworkUser _userPanel;
        AdminPanelSocialFrameworkChat _chatPanel;
        AdminPanelSocialFrameworkAlliances _alliancesPanel;
        AdminPanelSocialFrameworkPlayers _playersPanel;
        AdminPanelSocialFrameworkMessagingSystem _messagesPanel;
        AdminPanelSocialFrameworkDonations _donationsPanel;

        public AdminPanelSocialFramework(ConnectionManager connection, SocialManager socialManager, PlayersManager playersManager)
        {
            _connection = connection;
            _socialManager = socialManager;
            _playersManager = playersManager;
            _content = new StringBuilder();
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            _console = adminPanel.Console;
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Social Framework", this));

            // Cache nested panel
            _userPanel = new AdminPanelSocialFrameworkUser(_connection);
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

            var connected = Reflection.GetPrivateProperty<ConnectionManager, bool>(_connection, "IsSocketConnected");

            layout.CreateLabel("Social Framework");
            layout.CreateMargin();

            var connecting = Reflection.GetPrivateProperty<ConnectionManager, bool>(_connection, "IsSocketConnecting");
            var connectLabel = connecting ? "Connecting..." : "Connect";
            layout.CreateToggleButton(connectLabel, connected, value => {
                // Abort connection
                if(Reflection.GetPrivateProperty<ConnectionManager, bool>(_connection, "IsSocketConnecting"))
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

            layout.CreateOpenPanelButton("Players", _playersPanel, _playersManager != null && connected);
            layout.CreateOpenPanelButton("Chat", _chatPanel, ChatManager != null && connected);
            layout.CreateOpenPanelButton("Alliances", _alliancesPanel, AlliancesManager != null && connected);
            layout.CreateOpenPanelButton("Messages", _messagesPanel, MessagesManager != null && connected);

            ShowDonationsSection(layout, connected);
        }

        void ShowDonationsSection(AdminPanelLayout layout, bool connected)
        {
            bool donationsLoggedIn = DonationsManager != null && DonationsManager.IsLoggedIn && connected;
            bool donationLoginEnabled = DonationsManager != null && !DonationsManager.IsLoggedIn && connected;

            layout.CreateOpenPanelButton("Donations", _donationsPanel, donationsLoggedIn);
            Action<Error> completionHandler = err => {
                if(!Error.IsNullOrEmpty(err))
                {
                    _console.Print(err.ToString());
                }
                layout.Refresh();
            };
            Action<bool> onPress = state => {
                if(!state)
                {
                    DonationsManager.Login(completionHandler);
                }
            };
            layout.CreateToggleButton("Donations LoggedIn", donationsLoggedIn, onPress, donationLoginEnabled);
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

            protected virtual void OnInfoLoaded(AdminPanelLayout layout)
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

#endif
