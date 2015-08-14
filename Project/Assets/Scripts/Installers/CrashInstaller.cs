using Zenject;
using System;
using SocialPoint.Crash;

public class CrashInstaller : MonoInstaller
{
	[Serializable]
	public class SettingsData
	{
        public float SendInterval = CrashReporter.DefaultSendInterval;
        public bool ErrorLogActive = CrashReporter.DefaultErrorLogActive;
        public bool ExceptionLogActive = CrashReporter.DefaultExceptionLogActive;
        public bool AutoEnable = true;
	};
	
	public SettingsData Settings;

	public override void InstallBindings()
	{
        Container.BindInstance("crash_reporter_send_interval", Settings.SendInterval);
        Container.BindInstance("crash_reporter_error_log_active", Settings.ErrorLogActive);
        Container.BindInstance("crash_reporter_exception_log_active", Settings.ExceptionLogActive);
        Container.BindInstance("crash_reporter_autoenable", Settings.AutoEnable);
        Container.Bind<BreadcrumbManager>().ToSingle();

        var crash = Container.Instantiate<CrashReporter>();
        Container.BindInstance<ICrashReporter>(crash);
	}


}
