using System;
using System.Collections.Generic;
using System.Text;
using SocialPoint.AdminPanel;
using SocialPoint.AppEvents;
using SocialPoint.Utils;

namespace SocialPoint.Login
{
    public class AdminPanelLogin : IAdminPanelGUI, IAdminPanelConfigurer
    {
        readonly ILogin _login;
        readonly IDictionary<string, string> _environments;
        readonly IAppEvents _appEvents;
        AdminPanelLayout _layout;

        public AdminPanelLogin(ILogin login)
        {
            _login = login;
        }

        public AdminPanelLogin(ILogin login, IDictionary<string, string> envs, IAppEvents appEvents=null):this(login)
        {
            _login = login;
            _appEvents = appEvents;
            _environments = envs;
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Login", this));
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            // Inflate layout
            layout.CreateLabel("Login");
            layout.CreateMargin();
            
            if(_environments != null)
            {                
                layout.CreateLabel("Backend Environment");
                var envNames = new string[_environments.Count];
                int i = 0;
                StringBuilder envInfo = null;
                var itr = _environments.GetEnumerator();
                while(itr.MoveNext())
                {
                    var kvp = itr.Current;
                    envNames[i++] = kvp.Key;
                    var envUrl = StringUtils.FixBaseUri(kvp.Value);
                    if(envInfo == null && _login.BaseUrl == envUrl)
                    {
                        envInfo = new StringBuilder();
                        envInfo.Append("Name: ").AppendLine(kvp.Key);
                        envInfo.Append("URL: ").AppendLine(kvp.Value);
                    }
                }
                itr.Dispose();
                if(envInfo != null)
                {
                    layout.CreateTextArea(envInfo.ToString());
                }
                layout.CreateDropdown("Change environment", envNames, OnEnvironmentChange);
                layout.CreateMargin();
            }
            
            layout.CreateLabel("Actions");
            
            layout.CreateConfirmButton("Clear Stored User", _login.ClearStoredUser);
            var spLogin = _login as SocialPointLogin;
            if(spLogin != null)
            {
                layout.CreateConfirmButton("Clear Users Cache", spLogin.ClearUsersCache);
            }
            
            layout.CreateMargin();
            
            var loginInfo = new StringBuilder();
            loginInfo.Append("Base URL: ").AppendLine(_login.BaseUrl)
                .Append("User Id: ").AppendLine(_login.UserId.ToString())
                .Append("Session Id: ").AppendLine(_login.SessionId)
                .Append("Temp Id: ").AppendLine(_login.User.TempId)
                .Append("User name").AppendLine(_login.User.Name);

            if(spLogin != null)
            {
                loginInfo.Append("Security token: ").AppendLine(spLogin.SecurityToken);
            }

            var links = new StringBuilder();
            for(int i = 0, _loginUserLinksCount = _login.User.Links.Count; i < _loginUserLinksCount; i++)
            {
                var link = _login.User.Links[i];
                links.AppendLine(link.ToString());
            }
            
            var friends = new StringBuilder();
            for(int i = 0, _loginFriendsCount = _login.Friends.Count; i < _loginFriendsCount; i++)
            {
                var friend = _login.Friends[i];
                friends.AppendLine(friend.ToString());
            }
            
            layout.CreateLabel("Login Info");
            layout.CreateTextArea(loginInfo.ToString());
            
            layout.CreateLabel("Link Info");
            layout.CreateTextArea((links.Length > 0)? links.ToString() : "No links");
            
            layout.CreateLabel("Friends");
            layout.CreateVerticalScrollLayout().CreateTextArea((friends.Length > 0)? friends.ToString() : "No friends");

            _layout = layout;
        }

        void OnEnvironmentChange(string name)
        {
            string url;
            if(!_environments.TryGetValue(name, out url))
            {
                throw new InvalidOperationException(string.Format("Could not find url for env '{0}'", name));
            }
            _login.BaseUrl = url;
            if(_layout != null)
            {
                _layout.Refresh();
            }
            if(_appEvents != null)
            {
                _appEvents.RestartGame();
            }
        }
    }
}
