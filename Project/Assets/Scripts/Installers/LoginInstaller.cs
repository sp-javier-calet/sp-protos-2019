using System;
using System.Collections.Generic;
using SocialPoint.Dependency;
using SocialPoint.Login;
using SocialPoint.AdminPanel;
using SocialPoint.Network;
using SocialPoint.Hardware;
using SocialPoint.AppEvents;
using SocialPoint.ServerEvents;
using SocialPoint.Attributes;

public class LoginInstaller : Installer
{
    [Serializable]
    public class SettingsData
    {
        public BackendEnvironment Environment = BackendEnvironment.Develpoment;
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
        Container.Rebind<SocialPointLogin.LoginConfig>().ToSingleInstance<SocialPointLogin.LoginConfig>(new SocialPointLogin.LoginConfig {
            BaseUrl = Settings.Environment.GetUrl(),
            SecurityTokenErrors = (int)Settings.MaxSecurityTokenErrorRetries,
            ConnectivityErrors = (int)Settings.MaxConnectivityErrorRetries,
            EnableOnLinkConfirm = Settings.EnableLinkConfirmRetries
        });
        Container.BindInstance("login_timeout", Settings.Timeout);
        Container.BindInstance("login_activity_timeout", Settings.ActivityTimeout);
        Container.BindInstance("login_autoupdate_friends", Settings.AutoupdateFriends);
        Container.BindInstance("login_autoupdate_friends_photo_size", Settings.AutoupdateFriendsPhotoSize);
        Container.BindInstance("login_user_mappings_block", Settings.UserMappingsBlock);

        Container.Rebind<ILogin>().ToSingleMethod<SocialPointLogin>(CreateLogin);
        Container.Bind<IDisposable>().ToLookup<ILogin>();
    }

    SocialPointLogin CreateLogin()
    {
        var login = new SocialPointLogin(
            Container.Resolve<IHttpClient>(),
            Container.Resolve<SocialPointLogin.LoginConfig>());

        login.DeviceInfo = Container.Resolve<IDeviceInfo>();
        login.AppEvents = Container.Resolve<IAppEvents>();
        login.TrackEvent = Container.Resolve<IEventTracker>().TrackSystemEvent;
        login.Storage = Container.Resolve<IAttrStorage>("persistent");
        login.Timeout = Container.Resolve<float>("login_timeout", login.Timeout);
        login.ActivityTimeout = Container.Resolve<float>("login_activity_timeout", login.ActivityTimeout);
        login.AutoUpdateFriends = Container.Resolve<bool>("login_autoupdate_friends", login.AutoUpdateFriends);
        login.AutoUpdateFriendsPhotosSize = Container.Resolve<uint>("login_autoupdate_friends_photo_size", login.AutoUpdateFriendsPhotosSize);
        login.UserMappingsBlock = Container.Resolve<uint>("login_user_mappings_block", login.UserMappingsBlock);
        login.Language = Container.Resolve<string>("language", login.Language);

        var links = Container.ResolveArray<ILink>();
        for(var i = 0; i < links.Length; i++)
        {
            login.AddLink(links[i]);
        }

        return login;
    }
}
