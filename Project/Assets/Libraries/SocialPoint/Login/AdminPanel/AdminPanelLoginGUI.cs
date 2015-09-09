using UnityEngine;
using System.Text;
using System.Collections;
using SocialPoint.AdminPanel;

namespace SocialPoint.Login
{
    public class AdminPanelLoginGUI : IAdminPanelGUI, IAdminPanelConfigurer {

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
            StringBuilder loginInfo = new StringBuilder();
            loginInfo.Append("User Id: ").AppendLine(login.UserId.ToString())
                     .Append("Session Id: ").AppendLine(login.SessionId);
            layout.CreateTextArea(loginInfo.ToString());
        }

        public void OnCreateSocialPointLoginGUI(AdminPanelLayout layout, SocialPointLogin login)
        {
            // Collect and format login data

            StringBuilder loginInfo = new StringBuilder();
            loginInfo.AppendLine("Base URL: ").AppendLine(login.GetUrl(""))
                     .AppendLine("User Id: ").AppendLine(login.UserId.ToString())
                     .AppendLine("Session Id: ").AppendLine(login.SessionId)
                     .AppendLine("Security token: ").AppendLine(login.SecurityToken)
                     .AppendLine("Temp Id: ").AppendLine(login.User.TempId)
                     .AppendLine("User name").AppendLine(login.User.Name);

            StringBuilder links = new StringBuilder();
            foreach(var link in login.User.Links)
            {
                links.AppendLine(link.ToString());
            }

            StringBuilder friends = new StringBuilder();
            foreach(var friend in login.Friends)
            {
                friends.AppendLine(friend.ToString());
            }

            // Inflate layout
            layout.CreateLabel("SocialPoint Login");
            layout.CreateMargin();

            layout.CreateLabel("Login Info");
            layout.CreateTextArea(loginInfo.ToString());

            layout.CreateLabel("Link Info");
            layout.CreateTextArea((links.Length > 0)? links.ToString() : "No links");

            layout.CreateLabel("Friends");
            layout.CreateVerticalScrollLayout().CreateTextArea((friends.Length > 0)? friends.ToString() : "No friends");

            layout.CreateMargin();
            layout.CreateConfirmButton("Clear User Id", login.ClearUserId);

            layout.CreateConfirmButton("Clear Users Cache", login.ClearUsersCache);
        }
    }
}
