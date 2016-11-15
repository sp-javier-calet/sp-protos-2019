using System;
using System.Text;
using System.Collections.Generic;
using SocialPoint.AdminPanel;
using SocialPoint.Base;
using UnityEngine.UI;
using SocialPoint.WAMP;

namespace SocialPoint.Social
{
    public class AdminPanelSocialFramework : IAdminPanelConfigurer, IAdminPanelManagedGUI
    {
        readonly ConnectionManager _connection;
        readonly ChatManager _chat;
        readonly AlliancesManager _alliances;
        readonly StringBuilder _content;

        AdminPanelLayout _layout;
        AdminPanelConsole _console;

        AdminPanelSocialFrameworkUser _userPanel;
        AdminPanelSocialFrameworkChat _chatPanel;
        AdminPanelSocialFrameworkAlliance _alliancesPanel;

        public AdminPanelSocialFramework(ConnectionManager connection, ChatManager chat, AlliancesManager alliances)
        {
            _connection = connection;
            _chat = chat;
            _alliances = alliances;
            _content = new StringBuilder();
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            _console = adminPanel.Console;
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Social Framework", this));

            // Cache nested panel
            _userPanel = new AdminPanelSocialFrameworkUser(_connection);
            _chatPanel = new AdminPanelSocialFrameworkChat(_chat);
            _alliancesPanel = new AdminPanelSocialFrameworkAlliance(_alliances, _console);
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
                if(url == connectedUrl)
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
                    layout.CreateOpenPanelButton(room.Type, new AdminPanelChatRoom(room), room.Subscribed);
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


            #region Chat Inner classes

            class AdminPanelChatRoom : IAdminPanelManagedGUI
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

                public AdminPanelChatRoom(IChatRoom room)
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

        #endregion

        #region Alliances

        class AdminPanelSocialFrameworkAlliance : IAdminPanelGUI
        {
            readonly AdminPanelConsole _console;
            readonly AlliancesManager _alliances;

            readonly AdminPanelAllianceCreate _createPanel;
            readonly AdminPanelAllianceInfo _infoPanel;
            readonly AdminPanelAllianceSearch _searchPanel;
            readonly AdminPanelAllianceRanking _rankingPanel;

            public AdminPanelSocialFrameworkAlliance(AlliancesManager alliances, AdminPanelConsole console)
            {
                _alliances = alliances;
                _console = console;

                _createPanel = new AdminPanelAllianceCreate(alliances, console);
                _infoPanel = new AdminPanelAllianceInfo(alliances, console);
                _searchPanel = new AdminPanelAllianceSearch(alliances, console);
                _rankingPanel = new AdminPanelAllianceRanking(alliances, console);
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Alliance");
                layout.CreateMargin();

                CreateOwnAlliancePanel(layout);
                layout.CreateMargin();
                layout.CreateOpenPanelButton("Ranking", _rankingPanel);
                layout.CreateTextInput("Search alliances", value => {
                    _searchPanel.Search = value;
                    layout.Refresh();
                });
                layout.CreateOpenPanelButton("Search", _searchPanel, !string.IsNullOrEmpty(_searchPanel.Search));
                layout.CreateOpenPanelButton("Suggested alliances", _searchPanel);
               
            }

            void CreateOwnAlliancePanel(AdminPanelLayout layout)
            {
                var info = _alliances.AlliancePlayerInfo;
                if(info.IsInAlliance)
                {
                    _infoPanel.AllianceId = info.Id;
                    layout.CreateOpenPanelButton(info.Name, _infoPanel);
                    layout.CreateConfirmButton("Leave Alliance", () => _alliances.LeaveAlliance(err => {
                        if(Error.IsNullOrEmpty(err))
                        {
                            _console.Print("Alliance left");
                            layout.Refresh();
                        }
                        else
                        {
                            _console.Print(string.Format("Error leaving alliance. {0}", err));
                        }
                    }, true));
                }
                else
                {
                    layout.CreateOpenPanelButton("Create Alliance", _createPanel);
                }
            }

            #region Base Panels

            /// <summary>
            /// Base alliance panel.
            /// </summary>
            abstract class BaseAlliancePanel : IAdminPanelGUI
            {
                protected readonly AlliancesManager _alliances;
                protected readonly AdminPanelConsole _console;

                public BaseAlliancePanel(AlliancesManager alliances, AdminPanelConsole console)
                {
                    _alliances = alliances;
                    _console = console;
                }

                public abstract void OnCreateGUI(AdminPanelLayout layout);
            }

            /// <summary>
            /// Base alliance panel with http connection management
            /// </summary>
            abstract class BaseRequestAlliancePanel : BaseAlliancePanel
            {
                protected WAMPRequest _wampRequest;
                protected Error _wampRequestError;

                public BaseRequestAlliancePanel(AlliancesManager alliances, AdminPanelConsole console) : base(alliances, console)
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
            }

            #endregion

            class AdminPanelAllianceCreate : BaseAlliancePanel
            {
                AlliancesCreateData _data;

                StringBuilder _content;

                Alliance _allianceToEdit;

                public Alliance AllianceToEdit
                {
                    get
                    {
                        return _allianceToEdit;
                    }
                    set
                    {
                        _allianceToEdit = value;
                        SetCreateData(_allianceToEdit);
                    }
                }

                bool IsEditing
                {
                    get
                    {
                        return AllianceToEdit != null;
                    }
                }

                bool IsValidCreateData
                {
                    get
                    {
                        bool ret = true;
                        ret |= !string.IsNullOrEmpty(_data.Name);
                        ret |= !string.IsNullOrEmpty(_data.Description);
                        return ret;
                    }
                }

                public AdminPanelAllianceCreate(AlliancesManager alliances, AdminPanelConsole console) : base(alliances, console)
                {
                    _data = new AlliancesCreateData();
                    _content = new StringBuilder();
                }

                public override void OnCreateGUI(AdminPanelLayout layout)
                {
                    layout.CreateLabel(IsEditing ? "Edit Alliance" : "Create Alliance");
                    layout.CreateMargin();

                    StringValueInput(layout, "Name", _data.Name, value => {
                        _data.Name = value;
                    });
                    StringValueInput(layout, "Description", _data.Description, value => {
                        _data.Description = value;
                    });
                    IntValueInput(layout, "Required Score", _data.Requirement, value => {
                        _data.Requirement = value;
                    });
                    IntValueInput(layout, "Avatar", _data.Avatar, value => {
                        _data.Avatar = value;
                    });
                    IntValueInput(layout, "Access Type", _data.AccessType, value => {
                        _data.AccessType = value;
                    });
                    layout.CreateMargin();

                    if(IsEditing)
                    {
                        layout.CreateButton("Edit", () => {
                            if(IsValidCreateData)
                            {
                                _alliances.EditAlliance(AllianceToEdit, _data, err => {
                                    if(Error.IsNullOrEmpty(err))
                                    {
                                        _console.Print(string.Format("Alliance {0} successfully edited", _data.Name));
                                        ClearData();
                                        layout.ClosePanel();
                                    }
                                    else
                                    {
                                        _console.Print(string.Format("Error editing Alliance {0}. {1}", _data.Name, err));
                                    }
                                });
                                _console.Print("Editing alliance...");
                            }
                            else
                            {
                                _console.Print("Could not edit the alliance. Invalid alliance data.");
                            }
                        });
                    }
                    else
                    {
                        layout.CreateButton("Create", () => {
                            if(IsValidCreateData)
                            {
                                _alliances.CreateAlliance(_data, err => {
                                    if(Error.IsNullOrEmpty(err))
                                    {
                                        _console.Print(string.Format("Alliance {0} successfully created", _data.Name));
                                        ClearData();
                                        layout.ClosePanel();
                                    }
                                    else
                                    {
                                        _console.Print(string.Format("Error creating Alliance {0}. {1}", _data.Name, err));
                                    }
                                });
                                _console.Print("Creating alliance...");
                            }
                            else
                            {
                                _console.Print("Could not create the alliance. Invalid alliance data.");
                            }
                        });
                    }

                    layout.CreateMargin();

                    layout.CreateLabel("Summary");
                    layout.CreateVerticalScrollLayout().CreateTextArea(GetDataSummary());

                }

                void SetCreateData(Alliance alliance)
                {
                    if(alliance != null)
                    {
                        _data.Name = alliance.Name;
                        _data.Description = alliance.Description;
                        _data.Requirement = alliance.Requirement;
                        _data.Avatar = alliance.Avatar;
                        _data.AccessType = alliance.AccessType;
                    }
                    else
                    {
                        ClearData();
                    }
                }

                string GetDataSummary()
                {
                    _content.Length = 0;

                    _content
                        .Append("Name: ").AppendLine(_data.Name)
                        .Append("Description: ").AppendLine(_data.Description)
                        .Append("Requirement: ").AppendLine(_data.Requirement.ToString())
                        .Append("Avatar: ").AppendLine(_data.Avatar.ToString())
                        .Append("Type: ").AppendLine(_data.AccessType.ToString());
                    return _content.ToString();
                }

                void ClearData()
                {
                    _data = new AlliancesCreateData();
                }

                void StringValueInput(AdminPanelLayout layout, string label, string current, Action<string> onChanged)
                {
                    var hlayout = layout.CreateHorizontalLayout();
                    hlayout.CreateFormLabel(label);
                    hlayout.CreateTextInput(current, value => {
                        onChanged(value);
                        layout.Refresh();
                    }); 
                }

                void IntValueInput(AdminPanelLayout layout, string label, int current, Action<int> onChanged)
                {
                    var hlayout = layout.CreateHorizontalLayout();
                    hlayout.CreateFormLabel(label);
                    hlayout.CreateTextInput(current.ToString(), value => {
                        try
                        {
                            var parsed = int.Parse(value);
                            onChanged(parsed);
                            layout.Refresh();
                        }
                        catch(Exception)
                        {
                            _console.Print(string.Format("Invalid {0} value", label));
                            onChanged(current);
                            layout.Refresh();
                        }
                    });
                }
            }

            class AdminPanelAllianceInfo : BaseRequestAlliancePanel
            {
                public Alliance Alliance;

                public string AllianceId;

                readonly StringBuilder _content;
                readonly AdminPanelAllianceUserInfo _userPanel;
                readonly AdminPanelAllianceCreate _editPanel;

                public AdminPanelAllianceInfo(AlliancesManager alliances, AdminPanelConsole console) : base(alliances, console)
                {
                    _content = new StringBuilder();
                    _userPanel = new AdminPanelAllianceUserInfo(alliances, console);
                    _editPanel = new AdminPanelAllianceCreate(alliances, console);
                }

                public override void OnCreateGUI(AdminPanelLayout layout)
                {
                    if(Alliance != null)
                    {
                        _content.Length = 0;
                        _content
                            .Append("Id: ").AppendLine(Alliance.Id)
                            .Append("Name: ").AppendLine(Alliance.Name)
                            .Append("Description: ").AppendLine(Alliance.Description)
                            .Append("Avatar: ").AppendLine(Alliance.Avatar.ToString())
                            .Append("Type: ").AppendLine(Alliance.AccessType.ToString())
                            .Append("Activity: ").AppendLine(Alliance.ActivityIndicator.ToString())
                            .Append("Is New: ").AppendLine(Alliance.IsNewAlliance.ToString())
                            .Append("Score: ").AppendLine(Alliance.Score.ToString())
                            .Append("Required Score: ").AppendLine(Alliance.Requirement.ToString());
                        layout.CreateTextArea(_content.ToString());
                        layout.CreateMargin();

                        CreateMembersList(layout, "Members", Alliance, Alliance.GetMembers());
                        CreateMembersList(layout, "Candidates", Alliance, Alliance.GetCandidates());
                        CreateAllianceActions(layout);
                    }
                    else
                    {
                        if(_wampRequest == null)
                        {
                            _wampRequest = _alliances.LoadAllianceInfo(AllianceId,
                                (err, alliance) => {
                                    if(Error.IsNullOrEmpty(err))
                                    {
                                        Alliance = alliance;
                                        _console.Print(string.Format("Alliance {0} loaded successfully", alliance.Id));
                                        Cancel();
                                        layout.Refresh();
                                    }
                                    else
                                    {
                                        _console.Print(string.Format("Error loading user: {0}", err));
                                        _wampRequestError = err;
                                        layout.Refresh();
                                    }
                                });
                        }  
                        if(Error.IsNullOrEmpty(_wampRequestError))
                        {
                            layout.CreateLabel(string.Format("Loading alliance {0}...", AllianceId));
                        }
                        else
                        {
                            layout.CreateLabel("Load Alliance request failed.");
                            layout.CreateTextArea(_wampRequestError.ToString());
                            layout.CreateButton("Retry", () => {
                                Cancel();
                                layout.Refresh();
                            });
                        }
                    }
                }

                void CreateMembersList(AdminPanelLayout layout, string label, Alliance alliance, IEnumerator<AllianceMember> members)
                {
                    layout.CreateLabel(label);
                    while(members.MoveNext())
                    {
                        var member = members.Current;
                        var userLabel = string.Format("[{0}]: Lvl: {1} - S: {2} -- {3}", member.Name, member.Level, member.Score, member.Rank.ToString());
                        layout.CreateConfirmButton(userLabel, () => {
                            _userPanel.Alliance = alliance;
                            _userPanel.UserId = member.Uid;
                            layout.OpenPanel(_userPanel);
                        });
                    }
                    members.Dispose();
                    layout.CreateMargin();
                }

                void CreateAllianceActions(AdminPanelLayout layout)
                {
                    var ownAlliance = _alliances.AlliancePlayerInfo;
                    bool isInAlliance = ownAlliance.IsInAlliance;
                    var canEditAlliance = _alliances.Ranks.HasAllianceManagementPermission(ownAlliance.Rank);
                    var isOpenAlliance = _alliances.AccessTypes.IsPublic(Alliance.AccessType);
                    var canJoinAlliance = !isInAlliance && (isOpenAlliance || !Alliance.HasCandidate(ownAlliance.Id)); // TODO?

                    layout.CreateLabel("Actions");

                    var joinButtonLabel = isOpenAlliance ? "Join" : "Request Join";
                    var joinMessage = isOpenAlliance ? "Joining alliance..." : "Requesting join alliance...";

                    layout.CreateButton(joinButtonLabel, () => {
                        var data = _alliances.GetBasicDataFromAlliance(Alliance);
                        _console.Print(joinMessage);
                        _alliances.JoinAlliance(data, err => {
                            if(Error.IsNullOrEmpty(err))
                            {
                                Alliance = null;
                                _console.Print("User successfully joined alliance");
                                layout.Refresh();
                            }
                            else
                            {
                                _console.Print(string.Format("Error joining Alliance. {0}", err)); 
                            }
                        }, new JoinExtraData("AdminPanel"));
                    }, canJoinAlliance);

                    layout.CreateButton("Edit", () => {
                        _editPanel.AllianceToEdit = Alliance;
                        layout.OpenPanel(_editPanel);
                    }, canEditAlliance);
                }
            }

            class AdminPanelAllianceUserInfo : BaseRequestAlliancePanel
            {
                AllianceMember _member;

                public string UserId;
                public Alliance Alliance;

                readonly StringBuilder _content;

                public AdminPanelAllianceUserInfo(AlliancesManager alliances, AdminPanelConsole console) : base(alliances, console)
                {
                    _content = new StringBuilder();
                }

                public override void OnCreateGUI(AdminPanelLayout layout)
                {
                    layout.CreateLabel("User Info");
                    layout.CreateMargin();

                    if(_member != null)
                    {
                        _content.Length = 0;
                        _content
                            .Append("Id: ").AppendLine(_member.Uid)
                            .Append("Name: ").AppendLine(_member.Name)
                            .Append("Level: ").AppendLine(_member.Level.ToString())
                            .Append("Score: ").AppendLine(_member.Score.ToString())
                            .Append("Alliance: ").AppendLine(_member.AllianceName)
                            .Append("Alliance Id: ").AppendLine(_member.AllianceId)
                            .Append("Avatar: ").AppendLine(_member.AllianceAvatar.ToString())
                            .Append("Rank: ").AppendLine(_member.Rank.ToString());
                        layout.CreateVerticalLayout().CreateTextArea(_content.ToString());
                        layout.CreateMargin();

                        CreateAllianceActions(layout);
                    }
                    else
                    {
                        if(_wampRequest == null)
                        {
                            _wampRequest = _alliances.LoadUserInfo(UserId, 
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
                        }

                    }
                }

                void CreateAllianceActions(AdminPanelLayout layout)
                {
                    var ownAlliance = _alliances.AlliancePlayerInfo;
                    bool isOwnAlliance = ownAlliance.Id == Alliance.Id;
                    var playerHasHigherRank = _alliances.Ranks.Compare(_member.Rank, ownAlliance.Rank) > 0;
                    var userIsMember = Alliance.HasMember(_member.Uid);
                    var userIsCandidate = Alliance.HasCandidate(_member.Uid);
                    var rankActionsEnabled = isOwnAlliance && userIsMember && playerHasHigherRank;
                    var manageActionsEnabled = isOwnAlliance && userIsCandidate && _alliances.Ranks.HasMemberManagementPermission(ownAlliance.Rank);

                    layout.CreateLabel("Actions");

                    // Rank actions
                    layout.CreateButton("Promote", () => {
                        var newRank = _alliances.Ranks.GetPromoted(_member.Rank);
                        _alliances.PromoteMember(_member.Uid, newRank, err => OnResponse(err, "Promotion", () => Alliance.SetMemberRank(_member.Uid, newRank)));
                    }, rankActionsEnabled);

                    layout.CreateButton("Demote", () => {
                        var newRank = _alliances.Ranks.GetDemoted(_member.Rank);
                        _alliances.PromoteMember(_member.Uid, newRank, err => OnResponse(err, "Demotion", () => Alliance.SetMemberRank(_member.Uid, newRank)));
                    }, rankActionsEnabled);

                    layout.CreateButton("Kick", 
                        () => _alliances.KickMember(_member.Uid, err => OnResponse(err, "Kick", () => Alliance.RemoveMember(_member.Uid))), 
                        rankActionsEnabled);

                    // Management actions
                    layout.CreateButton("Accept Request", 
                        () => _alliances.AcceptCandidate(_member.Uid, err => OnResponse(err, "Accept candidate", () => Alliance.AcceptCandidate(_member.Uid))), 
                        manageActionsEnabled);
                    layout.CreateButton("Decline Request", 
                        () => _alliances.DeclineCandidate(_member.Uid, err => OnResponse(err, "Decline candidate", () => Alliance.RemoveCandidate(_member.Uid))), 
                        manageActionsEnabled);
                }

                void OnResponse(Error err, string action, Action onSuccess)
                {
                    if(Error.IsNullOrEmpty(err))
                    {
                        onSuccess();
                        _console.Print(string.Format("'{0}' request success", action));
                        _member = null;
                    }
                    else
                    {
                        _console.Print(string.Format("Error on '{0}' request. {1}", action, err));
                    }
                }
            }

            class AdminPanelAllianceRanking : BaseRequestAlliancePanel
            {
                AllianceRankingData _ranking;

                readonly AdminPanelAllianceInfo _infoPanel;

                public AdminPanelAllianceRanking(AlliancesManager alliances, AdminPanelConsole console) : base(alliances, console)
                {
                    _infoPanel = new AdminPanelAllianceInfo(alliances, console);
                }

                public override void OnCreateGUI(AdminPanelLayout layout)
                {
                    layout.CreateLabel("Ranking");
                    layout.CreateMargin();

                    if(_ranking != null)
                    {
                        var itr = _ranking.GetRanking();
                        while(itr.MoveNext())
                        {
                            var alliance = itr.Current;
                            var allianceLabel = string.Format("[{0}({1})]: {2}", alliance.Name, alliance.Id, alliance.Score);
                            layout.CreateConfirmButton(allianceLabel, () => {
                                _infoPanel.AllianceId = alliance.Id;
                                layout.OpenPanel(_infoPanel);
                            });
                        }
                        itr.Dispose();
                        layout.CreateMargin();
                    }
                    else
                    {
                        if(_wampRequest == null)
                        {
                            _wampRequest = _alliances.LoadRanking(
                                (err, ranking) => {
                                    if(Error.IsNullOrEmpty(err))
                                    {
                                        _ranking = ranking;
                                        _console.Print("Ranking loaded successfully");
                                        Cancel();
                                        layout.Refresh();
                                    }
                                    else
                                    {
                                        _console.Print(string.Format("Error loading ranking. {0} ", err));
                                        _wampRequestError = err;
                                        layout.Refresh();
                                    }
                                });
                        }  
                        if(Error.IsNullOrEmpty(_wampRequestError))
                        {
                            layout.CreateLabel("Loading ranking...");
                        }
                        else
                        {
                            layout.CreateLabel("Load user request failed.");
                            layout.CreateTextArea(_wampRequestError.ToString());
                            layout.CreateButton("Retry", () => {
                                Cancel();
                                layout.Refresh();
                            });
                        }

                    }
                }
            }

            class AdminPanelAllianceSearch : BaseRequestAlliancePanel
            {
                public string Search;

                AlliancesSearchData _search;

                readonly AdminPanelAllianceInfo _infoPanel;

                public AdminPanelAllianceSearch(AlliancesManager alliances, AdminPanelConsole console) : base(alliances, console)
                {
                    _infoPanel = new AdminPanelAllianceInfo(alliances, console);
                }

                public override void OnCreateGUI(AdminPanelLayout layout)
                {
                    layout.CreateLabel("Search results");
                    layout.CreateMargin();

                    if(_search != null)
                    {
                        var itr = _search.GetSearch();
                        while(itr.MoveNext())
                        {
                            var alliance = itr.Current;
                            var allianceLabel = string.Format("[{0}({1})]: {2}", alliance.Name, alliance.Id, alliance.Score);
                            layout.CreateConfirmButton(allianceLabel, () => {
                                _infoPanel.AllianceId = alliance.Id;
                                layout.OpenPanel(_infoPanel);
                            });
                        }
                        itr.Dispose();
                        layout.CreateMargin();
                    }
                    else
                    {
                        if(_wampRequest == null)
                        {
                            Action<Error, AlliancesSearchData> callback = (err, searchData) => {
                                if(Error.IsNullOrEmpty(err))
                                {
                                    _search = searchData;
                                    _console.Print("Search results loaded successfully");
                                    Cancel();
                                    layout.Refresh();
                                }
                                else
                                {
                                    _console.Print(string.Format("Error loading search results. {0} ", err));
                                    _wampRequestError = err;
                                    layout.Refresh();
                                }
                            };

                            if(string.IsNullOrEmpty(Search))
                            {
                                _wampRequest = _alliances.LoadSearchSuggested(callback);
                            }
                            else
                            {
                                _wampRequest = _alliances.LoadSearch(Search, callback);
                            }
                        }

                        if(Error.IsNullOrEmpty(_wampRequestError))
                        {
                            layout.CreateLabel("Loading ranking...");
                        }
                        else
                        {
                            layout.CreateLabel("Load user request failed.");
                            layout.CreateTextArea(_wampRequestError.ToString());
                            layout.CreateButton("Retry", () => {
                                Cancel();
                                layout.Refresh();
                            });
                        }
                    }
                }
            }
        }

        #endregion
    }
}