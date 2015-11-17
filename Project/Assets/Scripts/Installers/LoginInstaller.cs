﻿using Zenject;
using System;
using SocialPoint.Login;
using SocialPoint.Social;
using SocialPoint.AdminPanel;

public enum BackendEnvironment
{
    Develpoment,
    Production,
    Test
};

public static class BackendEnvironmentExtensions
{
    const string DevelopmentUrl = "http://pro-tech-bootstrap-000a.pro.tech.laicosp.net/api/v3";
    const string ProductionUrl = "http://pro-tech-bootstrap-000a.pro.tech.laicosp.net/api/v3";
    const string TestUrl = "http://pro-tech-bootstrap-000a.pro.tech.laicosp.net/api/v3";

    public static string GetUrl(this BackendEnvironment env)
    {
        switch(env)
        {
        case BackendEnvironment.Develpoment:
            return DevelopmentUrl;
        case BackendEnvironment.Production:
            return ProductionUrl;
        case BackendEnvironment.Test:
            return TestUrl;
        }
        return null;
    }
}

public class LoginInstaller : MonoInstaller
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
	
	public SettingsData Settings;

    [Inject]
    IFacebook Facebook;

    [Inject]
    IGameCenter GameCenter;

	public override void InstallBindings()
	{
        if(Facebook != null)
        {
            Container.Bind<ILink>().ToSingleMethod<FacebookLink>(CreateFacebookLink);
        }

        if(GameCenter != null)
        {
            Container.Bind<ILink>().ToSingleMethod<GameCenterLink>(CreateGameCenterLink);
        }

        Container.Bind<Login.LoginConfig>().ToInstance<Login.LoginConfig>(new Login.LoginConfig {
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

        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelLogin>();
	}

    FacebookLink CreateFacebookLink(InjectContext ctx)
    {
        return new FacebookLink(Facebook, Settings.FacebookLoginWithUi);
    }
        
    GameCenterLink CreateGameCenterLink(InjectContext ctx)
    {
        return new GameCenterLink(GameCenter);
    }
}