
using System;
using SocialPoint.Dependency;
using SocialPoint.Crash;
using SocialPoint.Utils;
using SocialPoint.Network;
using SocialPoint.Hardware;
using SocialPoint.Alert;
using SocialPoint.AdminPanel;

public class CrashInstaller : Installer
{
	[Serializable]
	public class SettingsData
	{
        public float SendInterval = CrashReporter.DefaultSendInterval;
        public bool ErrorLogActive = CrashReporter.DefaultErrorLogActive;
        public bool ExceptionLogActive = CrashReporter.DefaultExceptionLogActive;
        public bool EnableSendingCrashesBeforeLogin = CrashReporter.DefaultEnableSendingCrashesBeforeLogin;
        public int NumRetriesBeforeSendingCrashBeforeLogin = CrashReporter.DefaultNumRetriesBeforeSendingCrashBeforeLogin;
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
        Container.BindInstance("crash_reporter_send_interval", Settings.SendInterval);
        Container.BindInstance("crash_reporter_error_log_active", Settings.ErrorLogActive);
        Container.BindInstance("crash_reporter_exception_log_active", Settings.ExceptionLogActive);
        Container.BindInstance("crash_reporter_enable_sending_crashes_before_login", Settings.EnableSendingCrashesBeforeLogin);
        Container.BindInstance("crash_reporter_num_retries_before_sending_crash_before_login", Settings.NumRetriesBeforeSendingCrashBeforeLogin);
        Container.Rebind<BreadcrumbManager>().ToSingle<BreadcrumbManager>();
        Container.Rebind<ICrashReporter>().ToSingleMethod<CrashReporter>(CreateCrashReporter);
        Container.Bind<IDisposable>().ToLookup<ICrashReporter>();

        Container.Bind<IAdminPanelConfigurer>().ToSingleMethod<AdminPanelCrashReporter>(CreateAdminPanel);
    }

    AdminPanelCrashReporter CreateAdminPanel()
    {
        return new AdminPanelCrashReporter(
            Container.Resolve<ICrashReporter>(),
            Container.Resolve<BreadcrumbManager>());
    }

    CrashReporter CreateCrashReporter()
    {
        return new CrashReporter(
            Container.Resolve<ICoroutineRunner>(),
            Container.Resolve<IHttpClient>(),
            Container.Resolve<IDeviceInfo>(),
            Container.Resolve<BreadcrumbManager>(),
            Container.Resolve<IAlertView>());
    }

}
