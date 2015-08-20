using Zenject;
using SocialPoint.Events;
using SocialPoint.Network;
using SocialPoint.Hardware;
using SocialPoint.ServerSync;
using SocialPoint.Attributes;
using SocialPoint.Login;
using SocialPoint.Crash;
using UnityEngine;

class EventTracker : SocialPointEventTracker
{
    [InjectOptional("event_tracker_timeout")]
    public float InjectTimeout
    {
        set
        {
            Timeout = value;
        }
    }

    [InjectOptional("event_tracker_outofsync_interval")]
    public int InjectMaxOutOfSyncInterval
    {
        set
        {
            MaxOutOfSyncInterval = value;
        }
    }

    [InjectOptional("event_tracker_send_interval")]
    public int InjectSendInterval
    {
        set
        {
            SendInterval = value;
        }
    }

    [InjectOptional("event_tracker_backoff_multiplier")]
    public float InjectBackoffMultiplier
    {
        set
        {
            BackoffMultiplier = value;
        }
    }

    [Inject]
    public IHttpClient InjectHttpClient
    {
        set
        {
            HttpClient = value;
        }
    }

    [Inject]
    public IDeviceInfo InjectDeviceInfo
    {
        set
        {
            DeviceInfo = value;
        }
    }

    [Inject]
    public ICommandQueue InjectCommandQueue
    {
        set
        {
            CommandQueue = value;
        }
    }

    [InjectOptional]
    public BreadcrumbManager InjectBreadcrumbManager
    {
        set
        {
            BreadcrumbManager = value;
        }
    }
    
    [Inject]
    public ILogin InjectLogin
    {
        set
        {
            GetSessionId = () => {
                return value.SessionId;
            };
        }
    }

    public EventTracker(MonoBehaviour behaviour):base(behaviour)
    {
    }
}