using System.Text;
using SocialPoint.AdminPanel;
using SocialPoint.AppEvents;
using SocialPoint.Base;

namespace SocialPoint.Login
{
    public sealed class AdminPanelLogin : IAdminPanelGUI, IAdminPanelConfigurer
    {
        readonly ILogin _login;
        readonly IBackendEnvironment _environments;
        readonly IAppEvents _appEvents;

        public AdminPanelLogin(ILogin login)
        {
            _login = login;
        }

        public AdminPanelLogin(ILogin login, IBackendEnvironment environment, IAppEvents appEvents = null) : this(login)
        {
            _login = login;
            _appEvents = appEvents;
            _environments = environment;
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
            
                          
            layout.CreateLabel("Backend Environment");
            /*
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
                }*/
            layout.CreateOpenPanelButton("Change environment", new AdminPanelEnvironment(_login, _environments, _appEvents), _environments != null);
            layout.CreateMargin();
            
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
                .Append("Jenkins Forced Environment Url: ").AppendLine(EnvironmentSettings.Instance.EnvironmentUrl)
                .Append("User Id: ").AppendLine(_login.UserId.ToString())
                .Append("Session Id: ").AppendLine(_login.SessionId)
                .Append("Temp Id: ").AppendLine(_login.User.TempId)
                .Append("User name: ").AppendLine(_login.User.Name);

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
            layout.CreateTextArea((links.Length > 0) ? links.ToString() : "No links");
            
            layout.CreateLabel("Friends");
            layout.CreateVerticalScrollLayout().CreateTextArea((friends.Length > 0) ? friends.ToString() : "No friends");
        }
    }
}
