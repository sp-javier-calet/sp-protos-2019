using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.AppEvents;
using SocialPoint.AdminPanel;

namespace SocialPoint.Login
{

    public class AdminPanelLogin : IAdminPanelGUI, IAdminPanelConfigurer
    {
        ILogin _login;
        IDictionary<string, string> _environments;
        IAppEvents _appEvents;

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
            var currentUrl = login.GetUrl(string.Empty);

            // Inflate layout
            layout.CreateLabel("SocialPoint Login");
            layout.CreateMargin();

            if(_environments != null)
            {

                layout.CreateLabel("Backend Environment");
                var envNames = new string[_environments.Count];
                int i = 0;
                StringBuilder envInfo = null;
                foreach(var kvp in _environments)
                {
                    envNames[i++] = kvp.Key;
                    if(envInfo == null && currentUrl.StartsWith(kvp.Value))
                    {
                        envInfo = new StringBuilder();
                        envInfo.Append("Name: ").AppendLine(kvp.Key);
                        envInfo.Append("URL: ").AppendLine(kvp.Value);
                    }
                }
                if(envInfo != null)
                {
                    layout.CreateTextArea(envInfo.ToString());
                }
                layout.CreateDropdown("Change environment", envNames, OnEnvironmentChange);
                layout.CreateMargin();
            }

            layout.CreateLabel("Actions");

            layout.CreateConfirmButton("Clear Stored User", login.ClearStoredUser);
            layout.CreateConfirmButton("Clear Users Cache", login.ClearUsersCache);

            layout.CreateMargin();

            var loginInfo = new StringBuilder();
            loginInfo.AppendLine("Base URL: ").AppendLine(currentUrl)
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

            layout.CreateLabel("Login Info");
            layout.CreateTextArea(loginInfo.ToString());
            
            
            layout.CreateLabel("Link Info");
            layout.CreateTextArea((links.Length > 0)? links.ToString() : "No links");
            
            layout.CreateLabel("Friends");
            layout.CreateVerticalScrollLayout().CreateTextArea((friends.Length > 0)? friends.ToString() : "No friends");
        }

        void OnEnvironmentChange(string name)
        {
            var login = _login as SocialPointLogin;
            if(login == null)
            {
                throw new InvalidOperationException("Login type does not support backend environments");
            }
            string url = null;
            if(!_environments.TryGetValue(name, out url))
            {
                throw new InvalidOperationException(string.Format("Could not find url for env '{0}'", name));
            }
            var loginConfig = login.Config;
            loginConfig.BaseUrl = url;
            login.Config = loginConfig;
            if(_appEvents != null)
            {
                _appEvents.RestartGame();
            }
        }
    }
}
