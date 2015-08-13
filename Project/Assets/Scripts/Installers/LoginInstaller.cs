using Zenject;
using System;

public class LoginInstaller : MonoInstaller
{
	[Serializable]
	public class SettingsData
	{
		public string BaseUrl = "http://int-ds.socialpointgames.com/ds4/web/index_dev.php/api/v3";
        public float Timeout = Login.DefaultTimeout;
        public float ActivityTimeout = Login.DefaultActivityTimeout;
        public bool AutoupdateFriends = Login.DefaultAutoUpdateFriends;
        public uint AutoupdateFriendsPhotoSize = Login.DefaultAutoUpdateFriendsPhotoSize;
        public uint MaxRetries = Login.DefaultMaxLoginRetries;
        public uint UserMappingsBlock = Login.DefaultUserMappingsBlock;
	};
	
	public SettingsData Settings;

	public override void InstallBindings()
	{
		Container.BindInstance("base_url", Settings.BaseUrl);
        Container.BindInstance("login_timeout", Settings.Timeout);
        Container.BindInstance("login_activity_timeout", Settings.ActivityTimeout);
        Container.BindInstance("login_autoupdate_friends", Settings.AutoupdateFriends);
        Container.BindInstance("login_autoupdate_friends_photo_size", Settings.AutoupdateFriendsPhotoSize);
        Container.BindInstance("login_max_retries", Settings.MaxRetries);
        Container.BindInstance("login_user_mappings_block", Settings.UserMappingsBlock);

        Container.BindAllInterfacesToSingle<Login>();
	}


}
