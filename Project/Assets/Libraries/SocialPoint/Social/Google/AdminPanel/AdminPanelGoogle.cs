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

        public AdminPanelGoogle(IGoogle google)
        {
            _google = google;
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

            layout.CreateMargin();
            layout.CreateLabel("Achievements");
            layout.CreateOpenPanelButton("Achievements", new AdminPanelAchievementList(_google), connected);
            layout.CreateConfirmButton("Show Achievements UI", _google.ShowAchievementsUI, connected);

            layout.CreateMargin();
            layout.CreateLabel("Quests");
        }


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
            }
            
        }
    }
}