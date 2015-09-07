using Zenject;
using SocialPoint.AdminPanel;
using SocialPoint.Crash;
using SocialPoint.Network;
using SocialPoint.Hardware;
using SocialPoint.Login;
using SocialPoint.Events;
using UnityEngine;

class CrashReporter : SocialPointCrashReporter
{
    
    [Inject]
    public ILogin InjectLogin
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
    public IEventTracker InjectEventTracker
    {
        set
        {
            TrackEvent = value.TrackEvent;
        }
    }
    
    [InjectOptional("crash_reporter_send_interval")]
    public int InjectSendInterval
    {
        set
        {
            SendInterval = value;
        }
    }

    [InjectOptional("crash_reporter_error_log_active")]
    public bool InjectErrorLogActive
    {
        set
        {
            ErrorLogActive = value;
        }
    }
    
    [InjectOptional("crash_reporter_exception_log_active")]
    public bool InjectExceptionLogActive
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