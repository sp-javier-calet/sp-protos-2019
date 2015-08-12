using Zenject;
using System;
//using SocialPoint.Crash;

public class CrashInstaller : MonoInstaller
{
	[Serializable]
	public class SettingsData
	{

	};
	
	public SettingsData Settings;

	public override void InstallBindings()
	{
        //Container.BindAllInterfacesToSingle<CrashReporter>();
	}


}
