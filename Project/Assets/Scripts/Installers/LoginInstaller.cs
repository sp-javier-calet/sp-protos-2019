using Zenject;
using System;
using System.Collections.Generic;
using SocialPoint.Login;
using SocialPoint.Social;
using SocialPoint.AppEvents;
using SocialPoint.AdminPanel;

public class LoginInstaller : Installer
{
    [Serializable]
    public class SettingsData
    {
        public BackendEnvironment Environment = BackendEnvironment.Develpoment;
        public float Timeout = Login.DefaultTimeout;
        public float ActivityTimeout = Login.DefaultActivityTimeout;
        public bool AutoupdateFriends = Login.DefaultAutoUpdateFriends;
        public uint AutoupdateFriendsPhotoSize = Login.DefaultAutoUpdateFriendsPhotoSize;
        public uint MaxSecurityTokenErrorRetries = Login.DefaultMaxSecurityTokenErrorRetries;
        public uint MaxConnectivityErrorRetries = Login.DefaultMaxConnectivityErrorRetries;
        public bool EnableLinkConfirmRetries = Login.DefaultEnableLinkConfirmRetries;
        public uint UserMappingsBlock = Login.DefaultUserMappingsBlock;
        public bool FacebookLoginWithUi = false;
	};

    public SettingsData Settings = new SettingsData();

    [Inject]
    IFacebook _facebook;

    [Inject]
    IGameCenter _gameCenter;

    [Inject]
    IAppEvents _appEvents;

    public override void InstallBindings()
    {
        if(_facebook != null)
        {
            Container.Bind<ILink>().ToSingleMethod<FacebookLink>(CreateFacebookLink);
        }

        if(_gameCenter != null)
        {
            Container.Bind<ILink>().ToSingleMethod<GameCenterLink>(CreateGameCenterLink);
        }

        Container.Bind<IAdminPanelConfigurer>().ToSingleMethod<AdminPanelLogin>(CreateAdminPanel);

        InstallLogin();
	}

    void InstallLogin()
    {
        Container.Rebind<Login.LoginConfig>().ToInstance<Login.LoginConfig>(new Login.LoginConfig {
            BaseUrl = Settings.Environment.GetUrl(),
            SecurityTokenErrors = (int)Settings.MaxSecurityTokenErrorRetries, 
            ConnectivityErrors = (int)Settings.MaxConnectivityErrorRetries,
            EnableOnLinkConfirm = Settings.EnableLinkConfirmRetries }
        );
        Container.BindInstance("login_timeout", Settings.Timeout);
        Container.BindInstance("login_activity_timeout", Settings.ActivityTimeout);
        Container.BindInstance("login_autoupdate_friends", Settings.AutoupdateFriends);
        Container.BindInstance("login_autoupdate_friends_photo_size", Settings.AutoupdateFriendsPhotoSize);
        Container.BindInstance("login_user_mappings_block", Settings.UserMappingsBlock);
        
        Container.Rebind<ILogin>().ToSingle<Login>();
        Container.Bind<IDisposable>().ToSingle<Login>();
    }

    AdminPanelLogin CreateAdminPanel(InjectContext ctx)
    {
        var login = Container.Resolve<ILogin>();
        var appEvents = Container.Resolve<IAppEvents>();
        var envs = new Dictionary<string,string>();
        foreach(BackendEnvironment env in Enum.GetValues(typeof(BackendEnvironment)))
        {
            envs.Add(env.ToString(), env.GetUrl());
        }

        return new AdminPanelLogin(login, envs, appEvents);
    }

    FacebookLink CreateFacebookLink(InjectContext ctx)
    {
        return new FacebookLink(_facebook, Settings.FacebookLoginWithUi);
    }

    GameCenterLink CreateGameCenterLink(InjectContext ctx)
    {
        return new GameCenterLink(_gameCenter);
    }
}
