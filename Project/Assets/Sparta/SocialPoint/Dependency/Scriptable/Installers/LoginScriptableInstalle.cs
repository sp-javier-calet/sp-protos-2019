using System;
using System.Collections.Generic;
using SocialPoint.Dependency;
using SocialPoint.Login;
using SocialPoint.Network;
using SocialPoint.Hardware;
using SocialPoint.AppEvents;
using SocialPoint.ServerEvents;
using SocialPoint.Attributes;

public class LoginScriptableInstalle : ScriptableInstaller
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
    }
}
