#if ADMIN_PANEL 

using SocialPoint.AdminPanel;
using SocialPoint.Utils;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

namespace SocialPoint.Social
{
    public sealed class AdminPanelGoogle : IAdminPanelConfigurer, IAdminPanelGUI
    {
        readonly IGoogle _google;
        AdminPanelConsole _console;

        Toggle _toggleLogin;
        readonly AdminPanelGoogleLeaderboardIdHandler _leaderboardId;

        public AdminPanelGoogle(IGoogle google)
        {
            _google = google;
            _leaderboardId = new AdminPanelGoogleLeaderboardIdHandler();
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            if(_google == null)
            {
                return;
            }
            _console = adminPanel.Console;
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Google Play", this));
        }

        void ConsolePrint(string msg)
        {
            if(_console != null)
            {
                _console.Print(msg);
            }
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Google Play");
            layout.CreateMargin();
           
            _toggleLogin = layout.CreateToggleButton("Logged In", _google.IsConnected, status => {
                if(status)
                {
                    if(_google.IsConnected)
                    {
                        return;
                    }
                    ConsolePrint("Logging in to Google Play Games");

                    _google.Login(err => {
                        _toggleLogin.isOn = (err == null);
                        ConsolePrint("Login finished." + err);
                        layout.Refresh();
                    });
                }
                else
                {
                    _google.Logout(null);
                    _toggleLogin.isOn = false;
                    layout.Refresh();
                }
            });

            bool connected = _google.IsConnected;
            AdminPanelLayout groupLayout;

            layout.CreateMargin(2);
            layout.CreateLabel("User");
            var user = _google.User;
            var info = StringUtils.StartBuilder();
            if(user != null)
            {
                info.Append("Id:").AppendLine(user.UserId);
                info.Append("Name:").AppendLine(user.Name);
                info.Append("Age:").AppendLine(user.Age.ToString());
                info.Append("PhotoUrl:").AppendLine(user.PhotoUrl);
            }
            layout.CreateTextArea(StringUtils.FinishBuilder(info));

            layout.CreateMargin(2);
            layout.CreateLabel("Achievements");
            layout.CreateOpenPanelButton("Achievements", new AdminPanelGoogleAchievementList(_google, _console), connected);
            layout.CreateConfirmButton("Show Achievements UI", _google.ShowAchievementsUI, connected);

            layout.CreateMargin(2);
            layout.CreateLabel("Leaderboards");

            groupLayout = layout.CreateVerticalLayout();
            var ldbInput = groupLayout.CreateTextInput("leaderboard id", text => {
                _leaderboardId.Id = text;
            }, connected);
            ldbInput.text = _leaderboardId.Id;
            groupLayout.CreateOpenPanelButton("Leaderboard Info", new AdminPanelGoogleLeaderboard(_google, _leaderboardId, _console), connected);

            layout.CreateConfirmButton("Show Leaderboards UI", () => _google.ShowLeaderboardsUI(_leaderboardId.Id), connected);

            layout.CreateMargin(2);
            layout.CreateLabel("Quests");

            groupLayout = layout.CreateHorizontalLayout();
            var eventIdField = groupLayout.CreateTextInput("Event id", connected);
            groupLayout.CreateButton("+", () => _google.IncrementEvent(eventIdField.text), connected);

            layout.CreateConfirmButton("Show Quests UI", () => _google.ShowViewQuestsUI((evt, err) => ConsolePrint("Event " + evt + ". " + err)), connected);
        }

        #region Achievements panels

        class AdminPanelGoogleAchievementList : IAdminPanelGUI
        {
            readonly IGoogle _google;
            readonly AdminPanelConsole _console;

            public AdminPanelGoogleAchievementList(IGoogle google, AdminPanelConsole console)
            {
                _google = google;
                _console = console;
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Achievements");

                var itr = _google.Achievements.GetEnumerator();
                while(itr.MoveNext())
                {
                    var achievement = itr.Current;
                    layout.CreateOpenPanelButton(achievement.Name,
                        achievement.IsUnlocked ? ButtonColor.Green : ButtonColor.Default,
                        new AdminPanelGoogleAchievement(_google, achievement, _console));
                }
                itr.Dispose();
            }
        }

        class AdminPanelGoogleAchievement : IAdminPanelGUI
        {
            readonly GoogleAchievement _achievement;
            readonly IGoogle _google;
            readonly AdminPanelConsole _console;

            public AdminPanelGoogleAchievement(IGoogle google, GoogleAchievement achievement, AdminPanelConsole console)
            {
                _google = google;
                _achievement = achievement;
                _console = console;
            }

            void ConsolePrint(string msg)
            {
                if(_console != null)
                {
                    _console.Print(msg);
                }
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel(_achievement.Name);
                layout.CreateMargin();

                var info = StringUtils.StartBuilder();
                info.Append("Id:").AppendLine(_achievement.Id);
                info.Append("Name:").AppendLine(_achievement.Name);
                info.Append("Description:").AppendLine(_achievement.Description);
                info.Append("Incremental:").AppendLine(_achievement.IsIncremental.ToString());
                info.Append("Step ").Append(_achievement.CurrentSteps.ToString()).Append(" of ").AppendLine(_achievement.TotalSteps.ToString());
                info.Append("Unlocked:").AppendLine(_achievement.IsUnlocked.ToString());
                layout.CreateTextArea(StringUtils.FinishBuilder(info));
                layout.CreateMargin();

                layout.CreateButton(
                    _achievement.IsIncremental ? "Increment step" : "Unlock",
                    () => {
                        _achievement.CurrentSteps++;
                        _google.UpdateAchievement(_achievement, (achi, err) => {
                            ConsolePrint(string.Format("Updated Achievement {0}. {1}", achi.Name, err));
                            layout.Refresh();
                        });
                    },
                    !_achievement.IsUnlocked);

                layout.CreateButton("Reset", () => _google.ResetAchievement(_achievement, (achi, err) => {
                    ConsolePrint(string.Format("Reset Achievement {0}. {1}", achi.Name, err));
                    layout.Refresh();
                }));
            }
        }

        #endregion

        #region Leaderboard panels

        class AdminPanelGoogleLeaderboard :IAdminPanelGUI
        {
            readonly IGoogle _google;
            GoogleLeaderboard _leaderboard;
            readonly AdminPanelGoogleLeaderboardIdHandler _idHandler;
            Text _mainTitle;
            bool _isFriendOnly;
            bool _playerCentered;
            TimeScope _scope;
            readonly AdminPanelConsole _console;

            public AdminPanelGoogleLeaderboard(IGoogle google, AdminPanelGoogleLeaderboardIdHandler idHandler, AdminPanelConsole console)
            {
                _google = google;
                _idHandler = idHandler;
                _isFriendOnly = true;
                _playerCentered = true;
                _scope = TimeScope.Today;
                _console = console;
            }

            void ConsolePrint(string msg)
            {
                if(_console != null)
                {
                    _console.Print(msg);
                }
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                _mainTitle = layout.CreateLabel("Leaderboard not found");
                if(string.IsNullOrEmpty(_idHandler.Id))
                {
                    ConsolePrint("LeaderboardHandler id cannot be empty");
                    return;
                }
                _google.LoadLeaderboard(new GoogleLeaderboard(_idHandler.Id, _isFriendOnly, _playerCentered, _scope), 10, (ldb, err) => {
                    _leaderboard = ldb;
                    if(_leaderboard != null)
                    {
                        _mainTitle.text = _leaderboard.Title;

                        var info = StringUtils.StartBuilder();
                        info.Append("Id:").AppendLine(_leaderboard.Id);
                        info.Append("Title:").AppendLine(_leaderboard.Title);
                        info.Append("User score").AppendLine(_leaderboard.UserScore.ToString());

                        for(int i = 0, _leaderboardScoresCount = _leaderboard.Scores.Count; i < _leaderboardScoresCount; i++)
                        {
                            var entry = _leaderboard.Scores[i];
                            info.AppendLine(string.Format("{0}: {1} - {2}", entry.Rank, entry.Name, entry.Score));
                        }
                        layout.CreateTextArea(StringUtils.FinishBuilder(info));

                        layout.CreateMargin();
                        layout.CreateToggleButton("Friends only", _isFriendOnly, status => {
                            _isFriendOnly = status;
                            layout.Refresh();
                        });

                        layout.CreateToggleButton("Player Centered", _playerCentered, status => {
                            _playerCentered = status;
                            layout.Refresh();
                        });

                        layout.CreateConfirmButton("Show Leaderboard UI", () => _google.ShowLeaderboardsUI(_leaderboard.Id));
                    }
                    else
                    {
                        ConsolePrint("Error loading leaderboard " + _idHandler.Id + ". Error:" + err);
                        _mainTitle.text = "Leaderboard not found";
                    }
                });
            }
        }

        class AdminPanelGoogleLeaderboardIdHandler
        {
            const string kLeaderboardIdKey = "admin_google_leaderboard_id";
            string _id;

            public AdminPanelGoogleLeaderboardIdHandler()
            {
                _id = PlayerPrefs.GetString(kLeaderboardIdKey, string.Empty);
            }

            public string Id
            {
                get
                {
                    return _id;
                }
                set
                {
                    _id = value;
                    PlayerPrefs.SetString(kLeaderboardIdKey, _id);
                    PlayerPrefs.Save();
                }
            }
        }

        #endregion
    }
}

#endif
