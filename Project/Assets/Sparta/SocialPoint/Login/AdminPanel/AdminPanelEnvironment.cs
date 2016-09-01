using System;
using System.Collections.Generic;
using SocialPoint.AdminPanel;
using SocialPoint.AppEvents;

namespace SocialPoint.Login
{
    public sealed class AdminPanelEnvironment : IAdminPanelGUI
    {
        readonly ILogin _login;
        readonly IDictionary<string, string> _environments;
        readonly IAppEvents _appEvents;
        AdminPanelLayout _layout;

        public AdminPanelEnvironment(ILogin login, IDictionary<string, string> envs, IAppEvents appEvents)
        {
            _login = login;
            _appEvents = appEvents;
            _environments = envs;
        }

        public void OnCreateGUI( AdminPanelLayout layout )
        {   
            layout.CreateLabel( "Environments" );
            layout.CreateMargin( 2 );

            var itr = _environments.GetEnumerator();
            while(itr.MoveNext())
            {
                var kvp = itr.Current;
                layout.CreateButton(kvp.Key, () => OnEnvironmentChange(kvp.Key) );
            }
            itr.Dispose();

            _layout = layout;
        }

        void OnEnvironmentChange(string name)
        {
            string url;
            if(!_environments.TryGetValue(name, out url))
            {
                throw new InvalidOperationException(string.Format("Could not find url for env '{0}'", name));
            }
            _login.SetBaseUrl(url);
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