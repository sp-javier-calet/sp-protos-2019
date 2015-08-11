using Zenject;
using SocialPoint.Login;
using System;

public class LoginInstaller : MonoInstaller
{
	[Serializable]
	public class SettingsData
	{
		public string BaseUrl = "http://int-ds.socialpointgames.com/ds4/web/index_dev.php/api/v3";
	};
	
	public SettingsData Settings;

	public override void InstallBindings()
	{
		Container.BindInstance("base_url", Settings.BaseUrl);

		Container.Bind<string>().ToLookup<string>("base_url").WhenInjectedInto<SocialPointLogin>();
		Container.BindAllInterfacesToSingle<SocialPointLogin>();
	}
}
