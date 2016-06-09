
using System;
using SocialPoint.AdminPanel;
using SocialPoint.Alert;
using SocialPoint.AppEvents;
using SocialPoint.Crash;
using SocialPoint.Dependency;
using SocialPoint.Hardware;
using SocialPoint.Login;
using SocialPoint.Network;
using SocialPoint.ServerEvents;
using SocialPoint.Utils;

public class CrashInstaller : SubInstaller
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
        Container.Rebind<BreadcrumbManager>().ToSingle<BreadcrumbManager>();
        Container.Rebind<ICrashReporter>().ToMethod<SocialPointCrashReporter>(
            CreateCrashReporter, SetupCrashReporter);
        Container.Bind<IDisposable>().ToLookup<ICrashReporter>();

        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelCrashReporter>(CreateAdminPanel);
    }

    AdminPanelCrashReporter CreateAdminPanel()
    {
        return new AdminPanelCrashReporter(
            Container.Resolve<ICrashReporter>(),
            Container.Resolve<BreadcrumbManager>());
    }

    SocialPointCrashReporter CreateCrashReporter()
    {
        return new SocialPointCrashReporter(
            Container.Resolve<ICoroutineRunner>(),
            Container.Resolve<IHttpClient>(),
            Container.Resolve<IDeviceInfo>(),
            Container.Resolve<BreadcrumbManager>(),
            Container.Resolve<IAlertView>());
    }

    void SetupCrashReporter(SocialPointCrashReporter reporter)
    {
        var login = Container.Resolve<ILogin>();
        reporter.RequestSetup = login.SetupHttpRequest;
        reporter.GetUserId = () => login.UserId;
        reporter.TrackEvent = Container.Resolve<IEventTracker>().TrackUrgentSystemEvent;
        reporter.AppEvents = Container.Resolve<IAppEvents>();
        reporter.SendInterval = Settings.SendInterval;
        reporter.ErrorLogActive = Settings.ErrorLogActive;
        reporter.ExceptionLogActive = Settings.ExceptionLogActive;
        reporter.EnableSendingCrashesBeforeLogin = Settings.EnableSendingCrashesBeforeLogin;
        reporter.NumRetriesBeforeSendingCrashBeforeLogin = Settings.NumRetriesBeforeSendingCrashBeforeLogin;
        #if !UNITY_EDITOR
        (reporter as DeviceCrashReporter).Handler = Container.Resolve<NativeCallsHandler>();
        #endif
    }

}
