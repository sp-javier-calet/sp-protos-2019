using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms;
using SocialPoint.AdminPanel;
using SocialPoint.Base;
using System.Text;

namespace SocialPoint.Social
{
    public class AdminPanelGameCenter : IAdminPanelConfigurer, IAdminPanelGUI
    {
        IGameCenter _gameCenter;
        AdminPanel.AdminPanel _adminPanel;
        Toggle _toggleLogin;
        AdminPanelGameCenterAchievementList _achisPanel;

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
                    _adminPanel.Console.Print("Logging in to Game Center");
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
            _achisPanel = new AdminPanelGameCenterAchievementList(_gameCenter);
            layout.CreateOpenPanelButton("Achievements", _achisPanel, connected);
            layout.CreateButton("Show Achievements UI", _gameCenter.ShowAchievementsUI, connected);
            layout.CreateConfirmButton("Reset Achievements", ResetAchievements, connected);
        }

        void ResetAchievements()
        {
            _adminPanel.Console.Print("Reseting achievements...");
            _gameCenter.ResetAchievements((err) =>  {
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
            IGameCenter _gameCenter;
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

                foreach(var achievement in _gameCenter.Achievements)
                {
                    _layout.CreateOpenPanelButton(achievement.Id,
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
