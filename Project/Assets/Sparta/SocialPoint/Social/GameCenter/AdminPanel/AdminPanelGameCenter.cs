using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms;
using SocialPoint.AdminPanel;
using System.Text;

namespace SocialPoint.Social
{
    public class AdminPanelGameCenter : IAdminPanelConfigurer, IAdminPanelGUI
    {
        IGameCenter _gameCenter;
        AdminPanel.AdminPanel _adminPanel;
        Toggle _toggleLogin;

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
           
            _toggleLogin = layout.CreateToggleButton("Logged In", _gameCenter.IsConnected, (status) => {
                if(status)
                {
                    _adminPanel.Console.Print("Logging in to GameCenter Play Games");
                    _gameCenter.Login((err) => {
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
            var info = new StringBuilder();
            if(user != null)
            {
                info.Append("Id:").AppendLine(user.UserId);
                info.Append("Alias:").AppendLine(user.Alias);
                info.Append("DisplayName:").AppendLine(user.DisplayName);
                info.Append("Age:").AppendLine(user.Age.ToString());
                info.Append("Verification:").AppendLine(user.Verification == null ? "null" : user.Verification.ToString());
            }
            layout.CreateTextArea(info.ToString());

            layout.CreateMargin(2);
            layout.CreateLabel("Achievements");
            layout.CreateOpenPanelButton("Achievements", new AdminPanelGameCenterAchievementList(_gameCenter), connected);
            layout.CreateButton("Show Achievements UI", _gameCenter.ShowAchievementsUI, connected);
            layout.CreateConfirmButton("Reset Achievements", () => _gameCenter.ResetAchievements(), connected);
                         
        }

        #region Achievements panels

        class AdminPanelGameCenterAchievementList : IAdminPanelGUI
        {
            IGameCenter _gameCenter;

            public AdminPanelGameCenterAchievementList(IGameCenter gameCenter)
            {
                _gameCenter = gameCenter;
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Achievements");

                foreach(var achievement in _gameCenter.Achievements)
                {
                    layout.CreateOpenPanelButton(achievement.Id,
                        achievement.IsUnlocked ? ButtonColor.Green : ButtonColor.Default,
                        new AdminPanelAchievement(_gameCenter, achievement));
                }
            }
        }

        class AdminPanelAchievement : IAdminPanelGUI
        {
            GameCenterAchievement _achievement;
            IGameCenter _gameCenter;

            public AdminPanelAchievement(IGameCenter gameCenter, GameCenterAchievement achievement)
            {
                _gameCenter = gameCenter;
                _achievement = achievement;
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel(_achievement.Id);
                layout.CreateMargin();

                var info = new StringBuilder();
                info.Append("Percent:").AppendLine(_achievement.Percent.ToString());
                layout.CreateTextArea(info.ToString());
                layout.CreateMargin();

                layout.CreateButton(
                    "Increment 10%",
                    () => {
                        _achievement.Percent += 10;
                        _gameCenter.UpdateAchievement(_achievement, (achi, err) => {
                            layout.AdminPanel.Console.Print(string.Format("Updated Achievement {0}. {1}", achi.Id, err));
                            layout.Refresh();
                        });
                    },
                    !_achievement.IsUnlocked);
            }
        }

        #endregion

    }
}
