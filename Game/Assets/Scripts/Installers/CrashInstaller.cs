using Zenject;
using System;
using SocialPoint.Crash;

public class CrashInstaller : MonoInstaller
{
	[Serializable]
	public class SettingsData
	{
        public float SendInterval = CrashReporter.DefaultSendInterval;
	};
	
	public SettingsData Settings;

	public override void InstallBindings()
	{
        Container.BindInstance("crash_reporter_send_interval", Settings.SendInterval);
        Container.BindAllInterfacesToSingle<BreadcrumbManager>();
        Container.BindAllInterfacesToSingle<CrashReporter>();
	}


}
