#if ADMIN_PANEL 

using System;
using System.Text;
using System.Collections.Generic;
using SocialPoint.AdminPanel;
using SocialPoint.Base;

namespace SocialPoint.Social
{
    public class AdminPanelSocialFrameworkAlliances : IAdminPanelGUI
    {
        readonly AdminPanelConsole _console;
        readonly AlliancesManager _alliances;
        readonly SocialManager _socialManager;

        readonly AdminPanelAllianceCreate _createPanel;
        readonly AdminPanelAllianceInfo _infoPanel;
        readonly AdminPanelAllianceSearch _searchPanel;
        readonly AdminPanelAllianceRanking _rankingPanel;

        public AdminPanelSocialFrameworkAlliances(AlliancesManager alliances, PlayersManager playersManager, SocialManager socialManager, AdminPanelConsole console)
        {
            _alliances = alliances;
            _socialManager = socialManager;
            _console = console;

            _createPanel = new AdminPanelAllianceCreate(alliances, console);
            _infoPanel = new AdminPanelAllianceInfo(alliances, playersManager, socialManager, console);
            _searchPanel = new AdminPanelAllianceSearch(alliances, playersManager, socialManager, console);
            _rankingPanel = new AdminPanelAllianceRanking(alliances, playersManager, socialManager, console);
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Alliance");
            layout.CreateMargin();

            if(_socialManager.LocalPlayer.HasComponent<AlliancePlayerBasic>())
            {
                CreateOwnAlliancePanel(layout);
                layout.CreateMargin();
                layout.CreateOpenPanelButton("Ranking", _rankingPanel);
                layout.CreateTextInput(string.IsNullOrEmpty(_searchPanel.Filter) ? "Search alliances" : _searchPanel.Filter, value => {
                    _searchPanel.Filter = value;
                });
                layout.CreateOpenPanelButton("Search", _searchPanel);
            }
            else
            {
                layout.CreateLabel("Alliance Player Info not available");
            }
        }

        void CreateOwnAlliancePanel(AdminPanelLayout layout)
        {
            var info = _alliances.GetLocalBasicData();
            if(info.IsInAlliance())
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
        abstract class BaseAlliancePanel : AdminPanelSocialFramework.BaseRequestPanel
        {
            protected readonly AlliancesManager _alliances;
            protected readonly AdminPanelConsole _console;

            public BaseAlliancePanel(AlliancesManager alliances, AdminPanelConsole console)
            {
                _alliances = alliances;
                _console = console;
            }
        }

        #endregion

        class AdminPanelAllianceCreate : BaseAlliancePanel
        {
            Alliance _data;

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
                    ret &= !string.IsNullOrEmpty(_data.Name);
                    ret &= !string.IsNullOrEmpty(_data.Description);
                    ret &= !string.IsNullOrEmpty(_data.Message);
                    return ret;
                }
            }

            public AdminPanelAllianceCreate(AlliancesManager alliances, AdminPanelConsole console) : base(alliances, console)
            {
                _content = new StringBuilder();
            }

            public override void OnOpened()
            {
                ClearData();
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
                StringValueInput(layout, "Welcome", _data.Message, value => {
                    _data.Message = value;
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
                    _data.Message = alliance.Message;
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
                    .Append("Message: ").AppendLine(_data.Message)
                    .Append("Requirement: ").AppendLine(_data.Requirement.ToString())
                    .Append("Avatar: ").AppendLine(_data.Avatar.ToString())
                    .Append("Type: ").AppendLine(_data.AccessType.ToString());
                return _content.ToString();
            }

            void ClearData()
            {
                _data = _alliances.Factory.CreateAlliance(string.Empty, Attributes.Attr.InvalidDic);
            }

            static void StringValueInput(AdminPanelLayout layout, string label, string current, Action<string> onChanged)
            {
                var hlayout = layout.CreateHorizontalLayout();
                hlayout.CreateFormLabel(label);
                hlayout.CreateTextInput(current, onChanged); 
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
                    }
                    catch(Exception)
                    {
                        _console.Print(string.Format("Invalid {0} value", label));
                        onChanged(current);
                    }
                });
            }
        }

        class AdminPanelAllianceInfo : BaseAlliancePanel
        {
            public Alliance Alliance;

            public string AllianceId;

            readonly StringBuilder _content;
            readonly AdminPanelAllianceUserInfo _userPanel;
            readonly AdminPanelAllianceCreate _editPanel;

            public AdminPanelAllianceInfo(AlliancesManager alliances, PlayersManager playersManager, SocialManager socialManager, AdminPanelConsole console) : base(alliances, console)
            {
                _content = new StringBuilder();
                _userPanel = new AdminPanelAllianceUserInfo(alliances, playersManager, socialManager, console);
                _editPanel = new AdminPanelAllianceCreate(alliances, console);
            }

            public override void OnOpened()
            {
                base.OnOpened();
                Alliance = null;
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
                        .Append("Message: ").AppendLine(Alliance.Message)
                        .Append("Avatar: ").AppendLine(Alliance.Avatar.ToString())
                        .Append("Type: ").AppendLine(Alliance.AccessType.ToString())
                        .Append("Activity: ").AppendLine(Alliance.ActivityIndicator.ToString())
                        .Append("Is New: ").AppendLine(Alliance.IsNewAlliance.ToString())
                        .Append("Score: ").AppendLine(Alliance.Score.ToString())
                        .Append("Required Score: ").AppendLine(Alliance.Requirement.ToString());
                    layout.CreateTextArea(_content.ToString());
                    layout.CreateMargin();

                    CreateMembersList(layout, "Members", Alliance, Alliance.GetMembers(), Alliance.Members);
                    CreateMembersList(layout, "Candidates", Alliance, Alliance.GetCandidates(), Alliance.Candidates);
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

            void CreateMembersList(AdminPanelLayout layout, string label, Alliance alliance, IEnumerator<SocialPlayer> members, int count)
            {
                var foldout = layout.CreateFoldoutLayout(string.Format("{0} ({1})", label, count));
                while(members.MoveNext())
                {
                    var member = members.Current;
                    int rank = 0;
                    if(member.HasComponent<AlliancePlayerBasic>())
                    {
                        var basicAllianceComponent = member.GetComponent<AlliancePlayerBasic>();
                        rank = basicAllianceComponent.Rank;
                    }
                    var userLabel = string.Format("[{0}]: Lvl: {1} - S: {2} -- {3}", member.Name, member.Level, member.Score, rank);
                    foldout.CreateButton(userLabel, () => {
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
                var ownAlliance = _alliances.GetLocalBasicData();
                bool isInAlliance = ownAlliance.IsInAlliance();
                var canEditAlliance = _alliances.Ranks.HasPermission(ownAlliance.Rank, RankPermission.EditAlliance) && ownAlliance.Id == Alliance.Id;
                var isOpenAlliance = _alliances.AccessTypes.IsPublic(Alliance.AccessType);
                var canJoinAlliance = !isInAlliance && (isOpenAlliance || !Alliance.HasCandidate(ownAlliance.Id)); // TODO Use Member id instead of alliance

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

        class AdminPanelAllianceUserInfo : AdminPanelSocialFramework.BaseUserInfoPanel
        {
            public Alliance Alliance;
            readonly AlliancesManager _alliances;
            readonly SocialManager _socialManager;

            public AdminPanelAllianceUserInfo(AlliancesManager alliances, PlayersManager playersManager, SocialManager socialManager, AdminPanelConsole console) : base(playersManager, console)
            {
                _alliances = alliances;
                _socialManager = socialManager;
            }

            protected override void OnInfoLoaded(AdminPanelLayout layout)
            {
                base.OnInfoLoaded(layout);
                CreateAllianceActions(layout);
            }

            void CreateAllianceActions(AdminPanelLayout layout)
            {
                var memberId = UserId;
                var memberRank = 0;
                if(_member != null)
                {
                    memberRank = _member.GetComponent<AlliancePlayerBasic>().Rank;
                }

                var rankToPromote = _alliances.Ranks.GetPromotionRank(memberRank);
                var rankToDemote = _alliances.Ranks.GetDemotionRank(memberRank);

                var ownAlliance = _alliances.GetLocalBasicData();
                bool isOwnAlliance = ownAlliance.Id == Alliance.Id;

                var hasCandidateManagementPermissions = _alliances.Ranks.HasPermission(ownAlliance.Rank, RankPermission.ManageCandidates);
                var playerCanPromote = _alliances.Ranks.CanChangeRank(ownAlliance.Rank, memberRank, rankToPromote);
                var playerCanDemote = _alliances.Ranks.CanChangeRank(ownAlliance.Rank, memberRank, rankToDemote);

                var isOwnUser = memberId == _socialManager.LocalPlayer.Uid;
                var userIsMember = Alliance.HasMember(memberId);
                var userIsCandidate = Alliance.HasCandidate(memberId);
                var rankActionsEnabled = !isOwnUser && userIsMember && isOwnAlliance;
                var manageCandidateActionsEnabled = userIsCandidate && isOwnAlliance && hasCandidateManagementPermissions;

                layout.CreateLabel("Actions");

                // Rank actions
                layout.CreateButton("Promote", () => _alliances.PromoteMember(memberId, rankToPromote, err => OnResponse(layout, err, "Promotion", () => Alliance.SetMemberRank(memberId, rankToPromote))), rankActionsEnabled && playerCanPromote);

                layout.CreateButton("Demote", () => _alliances.PromoteMember(memberId, rankToDemote, err => OnResponse(layout, err, "Demotion", () => Alliance.SetMemberRank(memberId, rankToDemote))), rankActionsEnabled && playerCanDemote);

                layout.CreateButton("Kick", 
                    () => _alliances.KickMember(memberId, err => OnResponse(layout, err, "Kick", () => Alliance.RemoveMember(memberId))), 
                    rankActionsEnabled);

                // Management actions
                layout.CreateButton("Accept Request", 
                    () => _alliances.AcceptCandidate(memberId, err => OnResponse(layout, err, "Accept candidate", () => Alliance.AcceptCandidate(memberId))), 
                    manageCandidateActionsEnabled);
                layout.CreateButton("Decline Request", 
                    () => _alliances.DeclineCandidate(memberId, err => OnResponse(layout, err, "Decline candidate", () => Alliance.RemoveCandidate(memberId))), 
                    manageCandidateActionsEnabled);
            }

            void OnResponse(AdminPanelLayout layout, Error err, string action, Action onSuccess)
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
                layout.Refresh();
            }
        }

        class AdminPanelAllianceRanking : BaseAlliancePanel
        {
            AlliancesRanking _ranking;

            readonly AdminPanelAllianceInfo _infoPanel;

            public AdminPanelAllianceRanking(AlliancesManager alliances, PlayersManager playersManager, SocialManager socialManager, AdminPanelConsole console) : base(alliances, console)
            {
                _infoPanel = new AdminPanelAllianceInfo(alliances, playersManager, socialManager, console);
            }

            public override void OnOpened()
            {
                base.OnOpened();
                _ranking = null;
            }

            public override void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Ranking");
                layout.CreateMargin();

                if(_ranking != null)
                {
                    var itr = _ranking.GetEnumerator();
                    while(itr.MoveNext())
                    {
                        var alliance = itr.Current;
                        var allianceLabel = string.Format("[{0}({1})]: {2}", alliance.Name, alliance.Id, alliance.Score);
                        layout.CreateButton(allianceLabel, () => {
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
                        _wampRequest = _alliances.LoadRanking(null,
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

        class AdminPanelAllianceSearch : BaseAlliancePanel
        {
            public string Filter;

            AlliancesSearchResult _search;

            readonly AdminPanelAllianceInfo _infoPanel;

            public AdminPanelAllianceSearch(AlliancesManager alliances, PlayersManager playersManager, SocialManager socialManager, AdminPanelConsole console) : base(alliances, console)
            {
                _infoPanel = new AdminPanelAllianceInfo(alliances, playersManager, socialManager, console);
            }

            public override void OnOpened()
            {
                base.OnOpened();
                _search = null;
            }

            public override void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Search results");
                layout.CreateMargin();

                if(_search != null)
                {
                    var itr = _search.GetEnumerator();
                    while(itr.MoveNext())
                    {
                        var alliance = itr.Current;
                        var allianceLabel = string.Format("[{0}({1})]: {2}", alliance.Name, alliance.Id, alliance.Score);
                        layout.CreateButton(allianceLabel, () => {
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
                        Action<Error, AlliancesSearchResult> callback = (err, searchData) => {
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

                        var search = new AlliancesSearch();
                        search.Filter = Filter;
                        _wampRequest = _alliances.LoadSearch(search, callback);
                    }

                    if(Error.IsNullOrEmpty(_wampRequestError))
                    {
                        layout.CreateLabel("Loading search results...");
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
}

#endif
