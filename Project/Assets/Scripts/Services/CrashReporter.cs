using Zenject;
using SocialPoint.AdminPanel;
using SocialPoint.Crash;
using SocialPoint.Network;
using SocialPoint.Hardware;
using SocialPoint.Login;
using SocialPoint.Events;
using SocialPoint.AppEvents;
using UnityEngine;

class CrashReporter : SocialPointCrashReporter
{
    
    [Inject]
    ILogin injectLogin
    {
        set
        {
            RequestSetup = value.SetupHttpRequest;
            GetUserId = () => {
                return value.UserId;
            };
        }
    }
    
    [Inject]
    IEventTracker injectEventTracker
    {
        set
        {
            TrackEvent = value.TrackEvent;
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

    public CrashReporter(MonoBehaviour behaviour, IHttpClient client, IDeviceInfo deviceInfo, BreadcrumbManager breadcrumbs):
        base(behaviour, client, deviceInfo, breadcrumbs)
    {
    }

}