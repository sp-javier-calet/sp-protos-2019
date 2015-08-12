using Zenject;
using System;
using SocialPoint.Events;

public class EventsInstaller : MonoInstaller
{
	[Serializable]
	public class SettingsData
	{

	};
	
	public SettingsData Settings;

	public override void InstallBindings()
	{
        Container.BindAllInterfacesToSingle<SocialPointEventTracker>();
	}


}
