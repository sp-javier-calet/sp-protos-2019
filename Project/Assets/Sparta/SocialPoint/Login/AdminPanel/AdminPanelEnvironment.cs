using System;
using System.Collections.Generic;
using System.Text;
using SocialPoint.AdminPanel;
using SocialPoint.AppEvents;
using SocialPoint.Utils;

namespace SocialPoint.Login
{
    public class AdminPanelEnvironment : IAdminPanelGUI
    {
        readonly ILogin _login;
        readonly IDictionary<string, string> _environments;
        readonly IAppEvents _appEvents;

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

            StringBuilder envInfo = null;
            var itr = _environments.GetEnumerator();
            while(itr.MoveNext())
            {
                var kvp = itr.Current;
                layout.CreateButton(kvp.Key, () => {
                    OnEnvironmentChange(kvp.Key);
                } );
            }
        }

        void OnEnvironmentChange(string name)
        {
            string url;
            if(!_environments.TryGetValue(name, out url))
            {
                throw new InvalidOperationException(string.Format("Could not find url for env '{0}'", name));
            }
            _login.BaseUrl = url;
            BackendEnvironment environment = (BackendEnvironment) System.Enum.Parse( typeof( BackendEnvironment ), name );
            EnvironmentSerializer.Save(environment);
            if(_appEvents != null)
            {
                _appEvents.RestartGame();
            }
        }
    }
}