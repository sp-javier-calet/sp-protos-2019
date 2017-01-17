using System;
using System.Collections.Generic;
using SocialPoint.AdminPanel;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Dependency;
using SocialPoint.Hardware;
using SocialPoint.Locale;
using SocialPoint.Network;
using SocialPoint.ServerEvents;

namespace SocialPoint.Login
{
    public class LoginInstaller : SubInstaller
    {
        [Serializable]
        public class SettingsData
        {
            public bool UseEmpty;
            public float Timeout = SocialPointLogin.DefaultTimeout;
            public float ActivityTimeout = SocialPointLogin.DefaultActivityTimeout;
            public bool AutoupdateFriends = SocialPointLogin.DefaultAutoUpdateFriends;
            public uint AutoupdateFriendsPhotoSize = SocialPointLogin.DefaultAutoUpdateFriendsPhotoSize;
            public uint MaxSecurityTokenErrorRetries = SocialPointLogin.DefaultMaxSecurityTokenErrorRetries;
            public uint MaxConnectivityErrorRetries = SocialPointLogin.DefaultMaxConnectivityErrorRetries;
            public bool EnableLinkConfirmRetries = SocialPointLogin.DefaultEnableLinkConfirmRetries;
            public uint UserMappingsBlock = SocialPointLogin.DefaultUserMappingsBlock;
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            if(!Settings.UseEmpty)
            {
                Container.Rebind<SocialPointLogin.LoginConfig>().ToMethod<SocialPointLogin.LoginConfig>(CreateConfig);
                Container.Rebind<ILogin>().ToMethod<SocialPointLogin>(CreateLogin, SetupLogin);
            }
            else
            {
                Container.Rebind<ILogin>().ToMethod<EmptyLogin>(CreateEmptyLogin);
            }

            Container.Rebind<ILoginData>().ToLookup<ILogin>();
            Container.Bind<IDisposable>().ToLookup<ILogin>();

            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelLogin>(CreateAdminPanel);
        }

        SocialPointLogin.LoginConfig CreateConfig()
        {
            return new SocialPointLogin.LoginConfig {
                BaseUrl = Container.Resolve<IBackendEnvironment>().GetUrl(),
                SecurityTokenErrors = (int)Settings.MaxSecurityTokenErrorRetries,
                ConnectivityErrors = (int)Settings.MaxConnectivityErrorRetries,
                EnableOnLinkConfirm = Settings.EnableLinkConfirmRetries
            };
        }

        EmptyLogin CreateEmptyLogin()
        {
            return new EmptyLogin(null);
        }

        SocialPointLogin CreateLogin()
        {
            return new SocialPointLogin(
                Container.Resolve<IHttpClient>(),
                Container.Resolve<SocialPointLogin.LoginConfig>());
        }

        void SetupLogin(SocialPointLogin login)
        {
            login.DeviceInfo = Container.Resolve<IDeviceInfo>();
            login.AppEvents = Container.Resolve<IAppEvents>();
            login.TrackEvent = Container.Resolve<IEventTracker>().TrackSystemEvent;
            login.Storage = Container.Resolve<IAttrStorage>("persistent");
            login.Localization = Container.Resolve<ILocalizationManager>();
            login.Timeout = Settings.Timeout;
            login.ActivityTimeout = Settings.ActivityTimeout;
            login.AutoUpdateFriends = Settings.AutoupdateFriends;
            login.AutoUpdateFriendsPhotosSize = Settings.AutoupdateFriendsPhotoSize;
            login.UserMappingsBlock = Settings.UserMappingsBlock;

            var links = Container.ResolveList<ILink>();
            for(var i = 0; i < links.Count; i++)
            {
                login.AddLink(links[i]);
            }
        }

        AdminPanelLogin CreateAdminPanel()
        {
            var login = Container.Resolve<ILogin>();
            var appEvents = Container.Resolve<IAppEvents>();

            var envs = new Dictionary<string,string>();

            var environments = Container.Resolve<IBackendEnvironment>();
            if(environments != null)
            {
                for(var i = 0; i < environments.Environments.Length; ++i)
                {
                    var env = environments.Environments[i];
                    envs.Add(env.Name, env.Url);
                }
            }

            return new AdminPanelLogin(login, envs, appEvents);
        }
    }
}
