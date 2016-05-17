using SocialPoint.AdminPanel;
using SocialPoint.Base;
using SocialPoint.Utils;
using UnityEngine.UI;

namespace SocialPoint.Social
{
    public class AdminPanelGameCenter : IAdminPanelConfigurer, IAdminPanelGUI
    {
        readonly IGameCenter _gameCenter;
        AdminPanel.AdminPanel _adminPanel;
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
            _adminPanel = adminPanel;

            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Game Center", this));
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Game Center");
            layout.CreateMargin();
           
            _toggleLogin = layout.CreateToggleButton("Logged In", _gameCenter.IsConnected, status => {
                if(status)
                {
                    _adminPanel.Console.Print("Logging in to Game Center");
                    _gameCenter.Login(err => {
                        _toggleLogin.isOn = (err == null);
                        _adminPanel.Console.Print("Login finished." + err);
                    });
                }
                layout.Refresh();
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
            _achisPanel = new AdminPanelGameCenterAchievementList(_gameCenter);
            layout.CreateOpenPanelButton("Achievements", _achisPanel, connected);
            layout.CreateButton("Show Achievements UI", _gameCenter.ShowAchievementsUI, connected);
            layout.CreateConfirmButton("Reset Achievements", ResetAchievements, connected);
        }

        void ResetAchievements()
        {
            _adminPanel.Console.Print("Reseting achievements...");
            _gameCenter.ResetAchievements(err => {
                if(Error.IsNullOrEmpty(err))
                {
                    _adminPanel.Console.Print("Achievements were reset.");
                    _achisPanel.Refresh();
                }
                else
                {
                    _adminPanel.Console.Print("Error reseting achievements.");
                }
            });
        }

        #region Achievements panels

        class AdminPanelGameCenterAchievementList : IAdminPanelGUI
        {
            readonly IGameCenter _gameCenter;
            AdminPanelLayout _layout;

            public AdminPanelGameCenterAchievementList(IGameCenter gameCenter)
            {
                _gameCenter = gameCenter;
            }

            public void Refresh()
            {
                _layout.Refresh();
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
                        new AdminPanelAchievement(_gameCenter, achievement));
                }
                itr.Dispose();
            }
        }

        class AdminPanelAchievement : IAdminPanelGUI
        {
            readonly GameCenterAchievement _achievement;
            readonly IGameCenter _gameCenter;

            public AdminPanelAchievement(IGameCenter gameCenter, GameCenterAchievement achievement)
            {
                _gameCenter = gameCenter;
                _achievement = achievement;
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
                        var achi = (GameCenterAchievement)_achievement.Clone();
                        achi.Percent += AchievementPercent;
                        _gameCenter.UpdateAchievement(_achievement, (achi2, err) => {
                            layout.AdminPanel.Console.Print(string.Format("Updated Achievement {0}. {1}", achi2.Id, err));
                            layout.Refresh();
                        });
                    },
                    !_achievement.IsUnlocked);
            }
        }

        #endregion

    }
}
