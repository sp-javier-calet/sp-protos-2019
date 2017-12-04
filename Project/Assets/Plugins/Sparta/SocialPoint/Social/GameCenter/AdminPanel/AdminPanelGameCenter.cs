#if ADMIN_PANEL 

using System;
using SocialPoint.AdminPanel;
using SocialPoint.Base;
using SocialPoint.Utils;
using UnityEngine.UI;

namespace SocialPoint.Social
{
    public sealed class AdminPanelGameCenter : IAdminPanelConfigurer, IAdminPanelGUI
    {
        readonly IGameCenter _gameCenter;
        AdminPanelConsole _console;
        Toggle _toggleLogin;
        AdminPanelGameCenterAchievementList _achisPanel;

        const float AchievementPercent = 10;

        public AdminPanelGameCenter(IGameCenter gameCenter)
        {
            _gameCenter = gameCenter;
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            if(_gameCenter == null)
            {
                return;
            }
            _console = adminPanel.Console;

            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Game Center", this));
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
            layout.CreateLabel("Game Center");
            layout.CreateMargin();
           
            _toggleLogin = layout.CreateToggleButton("Logged In", _gameCenter.IsConnected, status => {
                if(status)
                {
                    if(_gameCenter.IsConnected)
                    {
                        return;
                    }

                    ConsolePrint("Logging in to Game Center");

                    _gameCenter.Login(err => {
                        _toggleLogin.isOn = _gameCenter.IsConnected;
                        ConsolePrint("Login finished." + err);
                        layout.Refresh();
                    });
                }
            });

            bool connected = _gameCenter.IsConnected;

            layout.CreateMargin(2);
            layout.CreateLabel("User");
            var user = _gameCenter.User;
            var info = StringUtils.StartBuilder();
            if(user != null)
            {
                info.Append("Id:").AppendLine(user.UserId);
                info.Append("Alias:").AppendLine(user.Alias);
                info.Append("DisplayName:").AppendLine(user.DisplayName);
                info.Append("Age:").AppendLine(user.Age.ToString());
                info.Append("Verification:").AppendLine(user.Verification == null ? "null" : user.Verification.ToString());
            }
            layout.CreateTextArea(StringUtils.FinishBuilder(info));

            layout.CreateMargin(2);
            layout.CreateLabel("Achievements");
            _achisPanel = new AdminPanelGameCenterAchievementList(_gameCenter, _console);
            layout.CreateOpenPanelButton("Achievements", _achisPanel, connected);
            layout.CreateButton("Show Achievements UI", _gameCenter.ShowAchievementsUI, connected);
            layout.CreateConfirmButton("Reset Achievements", ResetAchievements, connected);
        }

        void ResetAchievements()
        {
            ConsolePrint("Reseting achievements...");
            _gameCenter.ResetAchievements(err => {
                if(Error.IsNullOrEmpty(err))
                {
                    ConsolePrint("Achievements were reset.");
                    _achisPanel.Refresh();
                }
                else
                {
                    ConsolePrint("Error reseting achievements.");
                }
            });
        }

        #region Achievements panels

        class AdminPanelGameCenterAchievementList : IAdminPanelGUI
        {
            readonly IGameCenter _gameCenter;
            readonly AdminPanelConsole _console;
            AdminPanelLayout _layout;

            public AdminPanelGameCenterAchievementList(IGameCenter gameCenter, AdminPanelConsole console)
            {
                _gameCenter = gameCenter;
                _console = console;
            }

            public void Refresh()
            {
                if(_layout != null)
                {
                    _layout.Refresh();
                }
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                _layout = layout;
                _layout.CreateLabel("Achievements");

                var itr = _gameCenter.Achievements.GetEnumerator();
                while(itr.MoveNext())
                {
                    var achievement = itr.Current;
                    _layout.CreateOpenPanelButton(achievement.Title,
                        achievement.IsUnlocked ? ButtonColor.Green : ButtonColor.Default,
                        new AdminPanelAchievement(_gameCenter, achievement, _console));
                }
                itr.Dispose();
            }
        }

        class AdminPanelAchievement : IAdminPanelGUI
        {
            readonly GameCenterAchievement _achievement;
            readonly GameCenterAchievement _achievementCloned;
            readonly IGameCenter _gameCenter;
            readonly AdminPanelConsole _console;

            public AdminPanelAchievement(IGameCenter gameCenter, GameCenterAchievement achievement, AdminPanelConsole console)
            {
                _gameCenter = gameCenter;
                _achievement = achievement;
                _console = console;
                _achievementCloned = (GameCenterAchievement)achievement.Clone();
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
                layout.CreateLabel(_achievement.Title);
                layout.CreateMargin();

                var info = StringUtils.StartBuilder();
                info.Append("Id:").AppendLine(_achievement.Id);
                info.Append("Title:").AppendLine(_achievement.Title);
                info.Append("Hidden:").AppendLine(_achievement.Hidden.ToString());
                info.Append("Points:").AppendLine(_achievement.Points.ToString());
                info.Append("Unachieved Description:").AppendLine(_achievement.UnachievedDescription);
                info.Append("Achieved Description:").AppendLine(_achievement.AchievedDescription);
                info.Append("Percent:").AppendLine(_achievement.Percent.ToString());
                layout.CreateTextArea(StringUtils.FinishBuilder(info));
                layout.CreateMargin();

                var buttonText = string.Format("Increment {0}%", AchievementPercent);
                layout.CreateButton(
                    buttonText,
                    () => {
                        if(_gameCenter.IsAchievementUpdating(_achievement.Id))
                        {
                            return;
                        }

                        _achievementCloned.Percent = Math.Min(_achievementCloned.Percent + AchievementPercent, 100.0f);

                        _gameCenter.UpdateAchievement(_achievementCloned, (achi, err) => {
                            ConsolePrint(string.Format("- Updated {0}. - Percent: {1} - {2}", achi.Id, achi.Percent, err));
                            layout.Refresh();
                        });
                    },
                    !_achievement.IsUnlocked);
            }
        }

        #endregion

    }
}

#endif
