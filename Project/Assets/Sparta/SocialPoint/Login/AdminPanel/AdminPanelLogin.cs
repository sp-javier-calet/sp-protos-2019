#if ADMIN_PANEL 

using System.Text;
using SocialPoint.AdminPanel;
using SocialPoint.AppEvents;
using SocialPoint.Base;
using SocialPoint.Restart;

namespace SocialPoint.Login
{
    public sealed class AdminPanelLogin : IAdminPanelGUI, IAdminPanelConfigurer
    {
        readonly ILogin _login;
        readonly IBackendEnvironment _environments;
        readonly IRestarter _restarter;
        readonly AdminPanelLoginForcedErrors _forcedErrorPanel;
        readonly AdminPanelEnvironment _environmentsPanel;

        public AdminPanelLogin(ILogin login)
        {
            _login = login;
        }

        public AdminPanelLogin(ILogin login, IBackendEnvironment environment, IRestarter restarter = null) : this(login)
        {
            _login = login;
            _restarter = restarter;
            _environments = environment;

            _forcedErrorPanel = new AdminPanelLoginForcedErrors(login, restarter);
            _environmentsPanel = new AdminPanelEnvironment(_login, _environments, _restarter);
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

            layout.CreateOpenPanelButton("Change environment", _environmentsPanel, _environments != null);
            layout.CreateOpenPanelButton("Force Login Errors", _forcedErrorPanel);
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

            UnityEngine.Debug.Log(loginInfo);
        }
            
        public sealed class AdminPanelLoginForcedErrors : IAdminPanelGUI
        {
            readonly SocialPointLogin _login;
            readonly IRestarter _restarter;

            public AdminPanelLoginForcedErrors(ILogin login, IRestarter restarter)
            {
                _login = login as SocialPointLogin;
                _restarter = restarter;
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                if(_login == null)
                {
                    layout.CreateLabel("Unsupported ILogin implementation");
                    return;
                }

                layout.CreateLabel("Force Login Errors");
                layout.CreateMargin();

                var hlayout = layout.CreateHorizontalLayout();
                hlayout.CreateFormLabel("Error Code");
                var currentCode = _login.GetForcedErrorCode();
                var code = string.IsNullOrEmpty(currentCode) ? "none" : currentCode;
                hlayout.CreateTextInput(code, _login.SetForcedErrorCode);

                hlayout = layout.CreateHorizontalLayout();
                hlayout.CreateFormLabel("Error Type");
                var currentType = _login.GetForcedErrorType();
                var type = string.IsNullOrEmpty(currentType) ? "none" : currentType;
                hlayout.CreateTextInput(type, _login.SetForcedErrorType);

                layout.CreateButton("Clear", () => {
                    _login.SetForcedErrorCode(null);
                    _login.SetForcedErrorType(null);
                    layout.Refresh();
                });
                
                layout.CreateMargin();
                layout.CreateConfirmButton("Restart Game", () => _restarter.RestartGame());
            }
        }
    }
}

#endif
