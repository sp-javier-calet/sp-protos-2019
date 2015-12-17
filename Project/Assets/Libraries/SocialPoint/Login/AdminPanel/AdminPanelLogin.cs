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
            // Inflate layout
            layout.CreateLabel("Login");
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
                    if(envInfo == null && _login.BaseUrl == kvp.Value)
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
            
            layout.CreateConfirmButton("Clear Stored User", _login.ClearStoredUser);
            var spLogin = _login as SocialPointLogin;
            if(spLogin != null)
            {
                layout.CreateConfirmButton("Clear Users Cache", spLogin.ClearUsersCache);
            }
            
            layout.CreateMargin();
            
            var loginInfo = new StringBuilder();
            loginInfo.AppendLine("Base URL: ").AppendLine(_login.BaseUrl)
                .AppendLine("User Id: ").AppendLine(_login.UserId.ToString())
                .AppendLine("Session Id: ").AppendLine(_login.SessionId)
                .AppendLine("Temp Id: ").AppendLine(_login.User.TempId)
                .AppendLine("User name").AppendLine(_login.User.Name);

            if(spLogin != null)
            {
                loginInfo.AppendLine("Security token: ").AppendLine(spLogin.SecurityToken);
            }

            var links = new StringBuilder();
            foreach(var link in _login.User.Links)
            {
                links.AppendLine(link.ToString());
            }
            
            var friends = new StringBuilder();
            foreach(var friend in _login.Friends)
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
            string url = null;
            if(!_environments.TryGetValue(name, out url))
            {
                throw new InvalidOperationException(string.Format("Could not find url for env '{0}'", name));
            }
            _login.BaseUrl = url;
            if(_appEvents != null)
            {
                _appEvents.RestartGame();
            }
        }
    }
}
