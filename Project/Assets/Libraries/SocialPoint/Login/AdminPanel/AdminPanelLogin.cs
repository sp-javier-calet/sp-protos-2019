using UnityEngine;
using System.Text;
using System.Collections;
using SocialPoint.AdminPanel;

namespace SocialPoint.Login
{
    public class AdminPanelLogin : IAdminPanelGUI, IAdminPanelConfigurer {

        public ILogin _login;

        public AdminPanelLogin(ILogin login)
        {
            _login = login;
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Login", this));
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            SocialPointLogin spLogin = _login as SocialPointLogin;
            if(spLogin != null)
            {
                OnCreateSocialPointLoginGUI(layout, spLogin);
            }
            else
            {
                OnCreateDefaultLoginGUI(layout, _login);
            }
        }

        public void OnCreateDefaultLoginGUI(AdminPanelLayout layout, ILogin login)
        {
            layout.CreateLabel(_login.GetType().Name);
            // Collect and format login data
            var loginInfo = new StringBuilder();
            loginInfo.Append("User Id: ").AppendLine(login.UserId.ToString())
                     .Append("Session Id: ").AppendLine(login.SessionId);
            layout.CreateTextArea(loginInfo.ToString());
        }

        public void OnCreateSocialPointLoginGUI(AdminPanelLayout layout, SocialPointLogin login)
        {
            // Collect and format login data

            var loginInfo = new StringBuilder();
            loginInfo.AppendLine("Base URL: ").AppendLine(login.GetUrl(""))
                     .AppendLine("User Id: ").AppendLine(login.UserId.ToString())
                     .AppendLine("Session Id: ").AppendLine(login.SessionId)
                     .AppendLine("Security token: ").AppendLine(login.SecurityToken)
                     .AppendLine("Temp Id: ").AppendLine(login.User.TempId)
                     .AppendLine("User name").AppendLine(login.User.Name);

            var links = new StringBuilder();
            foreach(var link in login.User.Links)
            {
                links.AppendLine(link.ToString());
            }

            var friends = new StringBuilder();
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
            layout.CreateConfirmButton("Clear Stored User", login.ClearStoredUser);

            layout.CreateConfirmButton("Clear Users Cache", login.ClearUsersCache);
        }
    }
}
