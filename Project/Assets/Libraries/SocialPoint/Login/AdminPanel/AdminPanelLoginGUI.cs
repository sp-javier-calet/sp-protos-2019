using UnityEngine;
using System.Collections;
using SocialPoint.AdminPanel;

namespace SocialPoint.Login
{
    public class AdminPanelLoginGUI : AdminPanelGUI, AdminPanelConfigurer {

        public ILogin Login;

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("Login", this);
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            SocialPointLogin spLogin = Login as SocialPointLogin;
            if(spLogin != null)
            {
                OnCreateSocialPointLoginGUI(layout, spLogin);
            }
            else
            {
                OnCreateDefaultLoginGUI(layout, Login);
            }
        }

        public void OnCreateDefaultLoginGUI(AdminPanelLayout layout, ILogin login)
        {
            layout.CreateLabel(Login.GetType().Name);
            // Collect and format login data
            string loginInfo = "User Id: " + login.UserId + "\n" 
                             + "Session Id: " + login.SessionId + "\n";
            layout.CreateTextArea(loginInfo);
        }

        public void OnCreateSocialPointLoginGUI(AdminPanelLayout layout, SocialPointLogin login)
        {
            // Collect and format login data
            string loginInfo = "User Id: " + login.UserId + "\n" 
                + "Session Id: " + login.SessionId + "\n"
                + "Security token: " + login.SecurityToken + "\n"
                + "Temp Id: " + login.User.TempId + "\n"
                + "User name" + login.User.Name + "\n";

            string links = string.Empty;
            foreach(var link in login.User.Links)
            {
                links += link.ToString();
            }

            string friends = string.Empty;
            foreach(var friend in login.Friends)
            {
                friends += friend.ToString() + "\n";
            }

            // Inflate layout
            layout.CreateLabel("SocialPoint Login");
            layout.CreateMargin();

            layout.CreateLabel("Login Info");
            layout.CreateTextArea(loginInfo);

            layout.CreateLabel("Link Info");
            layout.CreateTextArea(links);

            layout.CreateLabel("Friends");
            layout.CreateVerticalScrollLayout().CreateTextArea(friends);


            layout.CreateMargin(2);
            layout.CreateButton("Clear User Id", () => {
                login.ClearUserId();
            });

            layout.CreateButton("Clear Users Cache", () => {
                login.ClearUsersCache();
            });
        }
    }
}
