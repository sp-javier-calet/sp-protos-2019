﻿using UnityEngine;
using UnityEngine.UI;
using SocialPoint.AdminPanel;
using System.Text;

namespace SocialPoint.Social
{
    public class AdminPanelGoogle : IAdminPanelConfigurer, IAdminPanelGUI
    {
        IGoogle _google;
        AdminPanel.AdminPanel _adminPanel;

        Toggle _toggleLogin;
        LeaderboardIdHandler _leaderboardId;

        public AdminPanelGoogle(IGoogle google)
        {
            _google = google;
            _leaderboardId = new LeaderboardIdHandler();
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            if(_google == null)
            {
                return;
            }
            _adminPanel = adminPanel;

            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Google Play", this));
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Google Play");
            layout.CreateMargin();
           
            _toggleLogin = layout.CreateToggleButton("Logged In", _google.IsConnected, (status) => {
                if(status)
                {
                    _adminPanel.Console.Print("Logging in to Google Play Games");
                    _google.Login((err) => {
                        _toggleLogin.isOn = (err == null);
                        _adminPanel.Console.Print("Login finished." + err);
                    });
                }
                else
                {
                    _google.Logout((err) => {
                    });
                }
                layout.Refresh();
            });

            bool connected = _google.IsConnected;
            AdminPanelLayout groupLayout;

            layout.CreateMargin(2);
            layout.CreateLabel("Achievements");
            layout.CreateOpenPanelButton("Achievements", new AdminPanelAchievementList(_google), connected);
            layout.CreateConfirmButton("Show Achievements UI", _google.ShowAchievementsUI, connected);

            layout.CreateMargin(2);
            layout.CreateLabel("Leaderboards");

            groupLayout = layout.CreateVerticalLayout();
            var ldbInput = groupLayout.CreateTextInput("leaderboard id", (text) => {
                _leaderboardId.Id = text;
            }, connected);
            ldbInput.text = _leaderboardId.Id;
            groupLayout.CreateOpenPanelButton("Leaderboard Info", new AdminPanelLeaderboard(_google, _leaderboardId), connected);

            layout.CreateConfirmButton("Show Leaderboards UI", _google.ShowLeaderboardsUI, connected);

            layout.CreateMargin(2);
            layout.CreateLabel("Quests");

            groupLayout = layout.CreateHorizontalLayout();
            var eventIdField = groupLayout.CreateTextInput("Event id", connected);
            groupLayout.CreateButton("+", () => _google.IncrementEvent(eventIdField.text), connected);

            layout.CreateConfirmButton("Show Quests UI", () => _google.ShowViewQuestsUI((evt, err) => {
                layout.AdminPanel.Console.Print("Event " + evt + ". " + err);
            }), connected);
        }

        #region Achievements panels

        class AdminPanelAchievementList : IAdminPanelGUI
        {
            IGoogle _google;

            public AdminPanelAchievementList(IGoogle google)
            {
                _google = google;
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Achievements");

                foreach(var achievement in _google.Achievements)
                {
                    layout.CreateOpenPanelButton(achievement.Name,
                        achievement.IsUnlocked ? ButtonColor.Green : ButtonColor.Default,
                        new AdminPanelAchievement(_google, achievement));
                }
            }
        }

        class AdminPanelAchievement : IAdminPanelGUI
        {
            GoogleAchievement _achievement;
            IGoogle _google;

            public AdminPanelAchievement(IGoogle google, GoogleAchievement achievement)
            {
                _google = google;
                _achievement = achievement;
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel(_achievement.Name);
                layout.CreateMargin();

                var info = new StringBuilder();
                info.Append("Id:").AppendLine(_achievement.Id);
                info.Append("Name:").AppendLine(_achievement.Name);
                info.Append("Description:").AppendLine(_achievement.Description);
                info.Append("Incremental:").AppendLine(_achievement.IsIncremental.ToString());
                info.Append("Step ").Append(_achievement.CurrentSteps.ToString()).Append(" of ").AppendLine(_achievement.TotalSteps.ToString());
                info.Append("Unlocked:").AppendLine(_achievement.IsUnlocked.ToString());
                layout.CreateTextArea(info.ToString());
                layout.CreateMargin();

                layout.CreateButton(
                    _achievement.IsIncremental ? "Increment step" : "Unlock",
                    () => {
                        _achievement.CurrentSteps++;
                        _google.UpdateAchievement(_achievement, (achi, err) => {
                            layout.AdminPanel.Console.Print(string.Format("Updated Achievement {0}. {1}", achi.Name, err));
                            layout.Refresh();
                        });
                    },
                    !_achievement.IsUnlocked);

                layout.CreateButton("Reset", () => _google.ResetAchievement(_achievement, (achi, err) => {
                    layout.AdminPanel.Console.Print(string.Format("Reset Achievement {0}. {1}", achi.Name, err));
                    layout.Refresh();
                }));
            }
            
        }

        #endregion

        #region Leaderboard panels

        class AdminPanelLeaderboard :IAdminPanelGUI
        {
            IGoogle _google;
            GoogleLeaderboard _leaderboard;
            LeaderboardIdHandler _idHandler;
            Text _mainTitle;
            bool _isFriendOnly;

            public AdminPanelLeaderboard(IGoogle google, LeaderboardIdHandler idHandler)
            {
                _google = google;
                _idHandler = idHandler;
                _isFriendOnly = true;
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                _mainTitle = layout.CreateLabel("Leaderboard not found");
                _google.LoadLeaderboard(new GoogleLeaderboard(_idHandler.Id, _isFriendOnly), (ldb, err) => {
                    _leaderboard = ldb;
                    if(_leaderboard != null)
                    {
                        _mainTitle.text = _leaderboard.Title;

                        var info = new StringBuilder();
                        info.Append("Id:").AppendLine(_leaderboard.Id);
                        info.Append("Title:").AppendLine(_leaderboard.Title);
                        info.Append("User score").AppendLine(_leaderboard.UserScore.ToString());

                        foreach(var entry in _leaderboard.Scores)
                        {
                            info.AppendLine(string.Format("{0}: {1} - {2}", entry.Rank, entry.Name, entry.Score));
                        }
                        layout.CreateTextArea(info.ToString());

                        layout.CreateMargin();
                        layout.CreateToggleButton("Friends only", _isFriendOnly, (status) => {
                            _isFriendOnly = status;
                            layout.Refresh();
                        });

                        layout.CreateConfirmButton("Show Leaderboard UI", () => _google.ShowLeaderboardsUI(_leaderboard.Id));
                    }
                    else
                    {
                        layout.AdminPanel.Console.Print("Error loading leaderboard " + _idHandler.Id + ". Error:" + err);
                        _mainTitle.text = "Leaderboard not found";
                    }
                });
            }
        }

        class LeaderboardIdHandler
        {
            const string kLeaderboardIdKey = "admin_leaderboard_id";
            string _id;

            public LeaderboardIdHandler()
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