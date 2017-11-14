using System;
using System.Collections.Generic;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Dependency;
using SocialPoint.Hardware;
using SocialPoint.Locale;
using SocialPoint.Network;
using SocialPoint.ServerEvents;
using SocialPoint.Restart;

#if ADMIN_PANEL
using SocialPoint.AdminPanel;
#endif

namespace SocialPoint.Login
{
    public class LoginInstaller : SubInstaller
    {
        public enum TypeLogin
        {
            Backend,
            Config,
            Empty
        }

        [Serializable]
        public class SettingsData
        {
            public TypeLogin TypeLogin;
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
            switch(Settings.TypeLogin)
            {
            case TypeLogin.Backend:
                Container.Rebind<SocialPointLogin.LoginConfig>().ToMethod<SocialPointLogin.LoginConfig>(CreateBackendConfig);
                Container.Rebind<ILogin>().ToMethod<SocialPointLogin>(CreateLogin, SetupLogin);
                break;
            case TypeLogin.Config:
                Container.Rebind<ILogin>().ToMethod<ConfigLogin>(CreateConfigLogin);
                break;
            case TypeLogin.Empty:
                Container.Rebind<ILogin>().ToMethod<EmptyLogin>(CreateEmptyLogin);
                break;
            default:
                throw new ArgumentOutOfRangeException();
            }

            Container.Rebind<ILoginData>().ToLookup<ILogin>();
            Container.Bind<IDisposable>().ToLookup<ILogin>();

            #if ADMIN_PANEL
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelLogin>(CreateAdminPanel);
            #endif
        }

        SocialPointLogin.LoginConfig CreateBackendConfig()
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

        ConfigLogin CreateConfigLogin()
        {
            var config = Container.Resolve<ConfigLoginEnvironment>();

            if(config == null)
            {
                throw new Exception("ConfigLogin configuration is required for ConfigLogin");
            }

            return new ConfigLogin(
                Container.Resolve<IHttpClient>(), 
                config.Endpoint);
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
            login.Restarter = Container.Resolve<IRestarter>();
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
                var link = links[i];
                login.AddLink(link);
            }
        }

        #if ADMIN_PANEL
        AdminPanelLogin CreateAdminPanel()
        {
            return new AdminPanelLogin(
                Container.Resolve<ILogin>(), 
                Container.Resolve<IBackendEnvironment>(),
                Container.Resolve<IRestarter>());
        }
        #endif
    }
}
