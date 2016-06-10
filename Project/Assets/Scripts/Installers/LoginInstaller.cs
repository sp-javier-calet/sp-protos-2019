using System;
using System.Collections.Generic;
using SocialPoint.Dependency;
using SocialPoint.Login;
using SocialPoint.Network;
using SocialPoint.Hardware;
using SocialPoint.AppEvents;
using SocialPoint.ServerEvents;
using SocialPoint.Attributes;

public class LoginInstaller : SubInstaller
{
    [Serializable]
    public class SettingsData
    {
        public BackendEnvironment Environment = BackendEnvironment.Development;
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
        if(!Container.HasInstalled<LoginAdminPanelInstaller>())
        {
            Container.Install<LoginAdminPanelInstaller>();
        }
        Container.Rebind<SocialPointLogin.LoginConfig>().ToInstance<SocialPointLogin.LoginConfig>(new SocialPointLogin.LoginConfig {
            BaseUrl = Settings.Environment.GetUrl(),
            SecurityTokenErrors = (int)Settings.MaxSecurityTokenErrorRetries,
            ConnectivityErrors = (int)Settings.MaxConnectivityErrorRetries,
            EnableOnLinkConfirm = Settings.EnableLinkConfirmRetries
        });
        Container.Rebind<ILogin>().ToMethod<SocialPointLogin>(CreateLogin, SetupLogin);
        Container.Bind<IDisposable>().ToLookup<ILogin>();
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
        login.Timeout = Settings.Timeout;
        login.ActivityTimeout = Settings.ActivityTimeout;
        login.AutoUpdateFriends = Settings.AutoupdateFriends;
        login.AutoUpdateFriendsPhotosSize = Settings.AutoupdateFriendsPhotoSize;
        login.UserMappingsBlock = Settings.UserMappingsBlock;
        login.Language = Container.Resolve<string>("language", login.Language);

        var links = Container.ResolveList<ILink>();
        for(var i = 0; i < links.Count; i++)
        {
            login.AddLink(links[i]);
        }
    }
}
