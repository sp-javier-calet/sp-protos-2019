using SocialPoint.AppEvents;
using SocialPoint.Crash;
using SocialPoint.Hardware;
using SocialPoint.Login;
using SocialPoint.Network;
using SocialPoint.ServerEvents;
using UnityEngine;
using Zenject;

class CrashReporter : SocialPointCrashReporter
{
    
    [Inject]
    ILogin injectLogin
    {
        set
        {
            RequestSetup = value.SetupHttpRequest;
            GetUserId = () => value.UserId;
        }
    }

    [Inject]
    IEventTracker injectEventTracker
    {
        set
        {
            TrackEvent = value.TrackUrgentSystemEvent;
        }
    }

    [Inject]
    IAppEvents injectAppEvents
    {
        set
        {
            AppEvents = value;
        }
    }

    [InjectOptional("crash_reporter_send_interval")]
    int injectSendInterval
    {
        set
        {
            SendInterval = value;
        }
    }

    [InjectOptional("crash_reporter_error_log_active")]
    bool injectErrorLogActive
    {
        set
        {
            ErrorLogActive = value;
        }
    }

    [InjectOptional("crash_reporter_exception_log_active")]
    bool injectExceptionLogActive
    {
        set
        {
            ExceptionLogActive = value;
        }
    }

    [InjectOptional("crash_reporter_enable_sending_crashes_before_login")]
    bool injectEnableSendingCrashesBeforeLogin
    {
        set
        {
            EnableSendingCrashesBeforeLogin = value;
        }
    }

    [InjectOptional("crash_reporter_num_retries_before_sending_crash_before_login")]
    int injectNumRetriesBeforeSendingCrashBeforeLogin
    {
        set
        {
            NumRetriesBeforeSendingCrashBeforeLogin = value;
        }
    }

    public CrashReporter(MonoBehaviour behaviour, IHttpClient client, IDeviceInfo deviceInfo, BreadcrumbManager breadcrumbs) :
        base(behaviour, client, deviceInfo, breadcrumbs)
    {
    }

}