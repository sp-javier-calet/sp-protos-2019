using Zenject;
using System;
using SocialPoint.AppEvents;

public class AppEventsInstaller : MonoInstaller
{
	[Serializable]
	public class SettingsData
	{

	};
	
	public SettingsData Settings;

	public override void InstallBindings()
	{
        Container.BindAllInterfacesToSingle<SocialPointAppEvents>();
	}

}
