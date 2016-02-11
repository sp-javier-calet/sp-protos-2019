using Zenject;
using System;
using System.Collections.Generic;
using SocialPoint.Login;
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
	};

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
        if(!Container.HasInstalled<LoginAdminPanelInstaller>())
        {
            Container.Install<LoginAdminPanelInstaller>();
        }
        Container.Rebind<Login.LoginConfig>().ToSingleInstance<Login.LoginConfig>(new Login.LoginConfig {
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

        Container.Rebind<ILogin>().ToSingle<Login>();
        Container.Bind<IDisposable>().ToLookup<ILogin>();
    }
}
