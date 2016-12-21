using System;
using SocialPoint.AdminPanel;
using SocialPoint.Alert;
using SocialPoint.AppEvents;
using SocialPoint.Base;
using SocialPoint.Dependency;
using SocialPoint.Hardware;
using SocialPoint.Login;
using SocialPoint.Network;
using SocialPoint.ServerEvents;
using SocialPoint.Utils;

namespace SocialPoint.Crash
{
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
            Container.Rebind<IBreadcrumbManager>().ToMethod<IBreadcrumbManager>(CreateBreadcrumbManager);
            Container.Rebind<ICrashReporter>().ToMethod<SocialPointCrashReporter>(
                CreateCrashReporter, SetupCrashReporter);
            Container.Bind<IDisposable>().ToLookup<ICrashReporter>();

            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelCrashReporter>(CreateAdminPanel);
        }

        AdminPanelCrashReporter CreateAdminPanel()
        {
            return new AdminPanelCrashReporter(
                Container.Resolve<ICrashReporter>(),
                Container.Resolve<IBreadcrumbManager>());
        }

        SocialPointCrashReporter CreateCrashReporter()
        {
            return new SocialPointCrashReporter(
                Container.Resolve<IUpdateScheduler>(),
                Container.Resolve<IHttpClient>(),
                Container.Resolve<IDeviceInfo>(),
                Container.Resolve<IBreadcrumbManager>(),
                Container.Resolve<IAlertView>());
        }

        IBreadcrumbManager CreateBreadcrumbManager()
        {
            var breadcrumbManager = new BreadcrumbManager();
            Log.BreadcrumbLogger = breadcrumbManager;
            return breadcrumbManager;
        }

        void SetupCrashReporter(SocialPointCrashReporter reporter)
        {
            reporter.LoginData = Container.Resolve<ILoginData>();
            reporter.TrackEvent = Container.Resolve<IEventTracker>().TrackUrgentSystemEvent;
            reporter.AppEvents = Container.Resolve<IAppEvents>();
            reporter.SendInterval = Settings.SendInterval;
            reporter.ErrorLogActive = Settings.ErrorLogActive;
            reporter.ExceptionLogActive = Settings.ExceptionLogActive;
            reporter.EnableSendingCrashesBeforeLogin = Settings.EnableSendingCrashesBeforeLogin;
            reporter.NumRetriesBeforeSendingCrashBeforeLogin = Settings.NumRetriesBeforeSendingCrashBeforeLogin;
            #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        reporter.NativeHandler = Container.Resolve<NativeCallsHandler>();
            #endif
        }

    }
}
