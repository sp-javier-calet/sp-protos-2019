using Zenject;
using System;
using SocialPoint.Crash;
using SocialPoint.AdminPanel;

public class CrashInstaller : MonoInstaller
{
	[Serializable]
	public class SettingsData
	{
        public float SendInterval = CrashReporter.DefaultSendInterval;
        public bool ErrorLogActive = CrashReporter.DefaultErrorLogActive;
        public bool ExceptionLogActive = CrashReporter.DefaultExceptionLogActive;
	};
	
	public SettingsData Settings;

	public override void InstallBindings()
	{
        Container.BindInstance("crash_reporter_send_interval", Settings.SendInterval);
        Container.BindInstance("crash_reporter_error_log_active", Settings.ErrorLogActive);
        Container.BindInstance("crash_reporter_exception_log_active", Settings.ExceptionLogActive);
        Container.Rebind<ICrashReporter>().ToSingle<CrashReporter>();
        Container.Bind<IDisposable>().ToLookup<ICrashReporter>();
        Container.Resolve<ICrashReporter>();

        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelCrashReporter>();
	}

}
