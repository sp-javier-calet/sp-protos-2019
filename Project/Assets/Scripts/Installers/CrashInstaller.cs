
using System;
using SocialPoint.Dependency;
using SocialPoint.Crash;
using SocialPoint.Utils;
using SocialPoint.Network;
using SocialPoint.Hardware;
using SocialPoint.Alert;
using SocialPoint.AdminPanel;
using SocialPoint.Login;
using SocialPoint.ServerEvents;
using SocialPoint.AppEvents;

public class CrashInstaller : Installer
{
	[Serializable]
	public class SettingsData
	{
        public float SendInterval = SocialPointCrashReporter.DefaultSendInterval;
        public bool ErrorLogActive = SocialPointCrashReporter.DefaultErrorLogActive;
        public bool ExceptionLogActive = SocialPointCrashReporter.DefaultExceptionLogActive;
        public bool EnableSendingCrashesBeforeLogin = SocialPointCrashReporter.DefaultEnableSendingCrashesBeforeLogin;
        public int NumRetriesBeforeSendingCrashBeforeLogin = SocialPointCrashReporter.DefaultNumRetriesBeforeSendingCrashBeforeLogin;
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
        Container.Rebind<ICrashReporter>().ToSingleMethod<SocialPointCrashReporter>(CreateCrashReporter);
        Container.Bind<IDisposable>().ToLookup<ICrashReporter>();

        Container.Bind<IAdminPanelConfigurer>().ToSingleMethod<AdminPanelCrashReporter>(CreateAdminPanel);
    }

    AdminPanelCrashReporter CreateAdminPanel()
    {
        return new AdminPanelCrashReporter(
            Container.Resolve<ICrashReporter>(),
            Container.Resolve<BreadcrumbManager>());
    }

    SocialPointCrashReporter CreateCrashReporter()
    {
        var reporter = new SocialPointCrashReporter(
            Container.Resolve<ICoroutineRunner>(),
            Container.Resolve<IHttpClient>(),
            Container.Resolve<IDeviceInfo>(),
            Container.Resolve<BreadcrumbManager>(),
            Container.Resolve<IAlertView>());

        var login = Container.Resolve<ILogin>();
        reporter.RequestSetup = login.SetupHttpRequest;
        reporter.GetUserId = () => login.UserId;
        reporter.TrackEvent = Container.Resolve<IEventTracker>().TrackUrgentSystemEvent;
        reporter.AppEvents = Container.Resolve<IAppEvents>();
        reporter.SendInterval = Container.Resolve<float>("crash_reporter_send_interval", reporter.SendInterval);
        reporter.ErrorLogActive = Container.Resolve<bool>("crash_reporter_error_log_active", reporter.ErrorLogActive);
        reporter.ExceptionLogActive = Container.Resolve<bool>("crash_reporter_exception_log_active", reporter.ExceptionLogActive);
        reporter.EnableSendingCrashesBeforeLogin = Container.Resolve<bool>("crash_reporter_enable_sending_crashes_before_login", reporter.EnableSendingCrashesBeforeLogin);
        reporter.NumRetriesBeforeSendingCrashBeforeLogin = Container.Resolve<int>("crash_reporter_num_retries_before_sending_crash_before_login", reporter.NumRetriesBeforeSendingCrashBeforeLogin);

        return reporter;
    }

}
