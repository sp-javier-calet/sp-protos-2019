using Zenject;
using SocialPoint.Crash;
using SocialPoint.Network;
using SocialPoint.Hardware;
using SocialPoint.Login;
using SocialPoint.Events;
using UnityEngine;

class CrashReporter : SocialPointCrashReporter
{
    
    [InjectOptional("crash_reporter_send_interval")]
    public int InjectSendInterval
    {
        set
        {
            SendInterval = value;
        }
    }

    [InjectOptional]
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

    [InjectOptional]
    public IEventTracker InjectEventTracker
    {
        set
        {
            TrackEvent = value.TrackEvent;
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

    private bool _autoEnable = false;
    [InjectOptional("crash_reporter_autoenable")]
    public bool InjectAutoEnable
    {
        set
        {
            _autoEnable = value;
        }
    }

    public CrashReporter(MonoBehaviour behaviour, IHttpClient client, IDeviceInfo deviceInfo, BreadcrumbManager breadcrumbs):
        base(behaviour, client, deviceInfo, breadcrumbs)
    {
    }

    [PostInject]
    public void PostInject()
    {
        if(_autoEnable)
        {
            Enable();
        }
    }
}