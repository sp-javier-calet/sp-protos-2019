using SocialPoint.AppEvents;
using SocialPoint.Crash;
using SocialPoint.Hardware;
using SocialPoint.Login;
using SocialPoint.Network;
using SocialPoint.ServerEvents;
using SocialPoint.Alert;
using SocialPoint.Utils;
using SocialPoint.Dependency;

class CrashReporter : SocialPointCrashReporter
{
    public CrashReporter(ICoroutineRunner runner, IHttpClient client, IDeviceInfo deviceInfo, BreadcrumbManager breadcrumbs, IAlertView alertView = null) :
    base(runner, client, deviceInfo, breadcrumbs, alertView)
    {
        var login = ServiceLocator.Instance.Resolve<ILogin>();
        RequestSetup = login.SetupHttpRequest;
        GetUserId = () => login.UserId;
        TrackEvent = ServiceLocator.Instance.Resolve<IEventTracker>().TrackUrgentSystemEvent;
        AppEvents = ServiceLocator.Instance.Resolve<IAppEvents>();
        SendInterval = ServiceLocator.Instance.OptResolve<float>("crash_reporter_send_interval", SendInterval);
        ErrorLogActive = ServiceLocator.Instance.OptResolve<bool>("crash_reporter_error_log_active", ErrorLogActive);
        ExceptionLogActive = ServiceLocator.Instance.OptResolve<bool>("crash_reporter_exception_log_active", ExceptionLogActive);
        EnableSendingCrashesBeforeLogin = ServiceLocator.Instance.OptResolve<bool>("crash_reporter_enable_sending_crashes_before_login", EnableSendingCrashesBeforeLogin);
        NumRetriesBeforeSendingCrashBeforeLogin = ServiceLocator.Instance.OptResolve<int>("crash_reporter_num_retries_before_sending_crash_before_login", NumRetriesBeforeSendingCrashBeforeLogin);
    }

}