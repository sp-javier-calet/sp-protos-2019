﻿using SocialPoint.AdminPanel;
using SocialPoint.AppEvents;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.Login
{
    public sealed class AdminPanelEnvironment : IAdminPanelGUI
    {
        readonly ILogin _login;
        readonly IBackendEnvironment _environments;
        readonly IAppEvents _appEvents;
        AdminPanelLayout _layout;

        public AdminPanelEnvironment(ILogin login, IBackendEnvironment environment, IAppEvents appEvents)
        {
            _login = login;
            _appEvents = appEvents;
            _environments = environment;
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {   
            var currentUrl = _login.BaseUrl;
            layout.CreateLabel("Environments");
            layout.CreateMargin(2);
            for(var i = 0; i < _environments.Environments.Length; ++i)
            {
                var current = _environments.Environments[i];
                var baseUrl = StringUtils.FixBaseUri(current.Url);
                layout.CreateButton(current.Name, () => OnEnvironmentChange(current), currentUrl != baseUrl);
            }

            layout.CreateMargin();
            layout.CreateButton("Clear Stored", ClearSelected, !string.IsNullOrEmpty(_environments.Storage.Selected));

            _layout = layout;
        }

        void OnEnvironmentChange(Environment env)
        {
            _environments.Storage.Selected = env.Name;
            _login.SetBaseUrl(env.Url);
            Restart();
        }

        void ClearSelected()
        {
            _environments.Storage.Selected = null;
            _login.SetBaseUrl(_environments.GetUrl());
            Restart();
        }

        void Restart()
        {
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
