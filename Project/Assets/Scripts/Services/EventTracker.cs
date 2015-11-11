using Zenject;
using SocialPoint.ServerEvents;
using SocialPoint.Network;
using SocialPoint.Hardware;
using SocialPoint.ServerSync;
using SocialPoint.Attributes;
using SocialPoint.Login;
using SocialPoint.Crash;
using SocialPoint.AppEvents;
using SocialPoint.Base;
using SocialPoint.GameLoading;
using UnityEngine;
using System;

class EventTracker : SocialPointEventTracker
{
    [InjectOptional("event_tracker_timeout")]
    float injectTimeout
    {
        set
        {
            Timeout = value;
        }
    }

    [InjectOptional("event_tracker_outofsync_interval")]
    int injectMaxOutOfSyncInterval
    {
        set
        {
            MaxOutOfSyncInterval = value;
        }
    }

    [InjectOptional("event_tracker_send_interval")]
    int injectSendInterval
    {
        set
        {
            SendInterval = value;
        }
    }

    [InjectOptional("event_tracker_backoff_multiplier")]
    float injectBackoffMultiplier
    {
        set
        {
            BackoffMultiplier = value;
        }
    }

    [Inject]
    IHttpClient injectHttpClient
    {
        set
        {
            HttpClient = value;
        }
    }

    [Inject]
    IDeviceInfo injectDeviceInfo
    {
        set
        {
            DeviceInfo = value;
        }
    }

    [Inject]
    ICommandQueue injectCommandQueue
    {
        set
        {
            CommandQueue = value;
        }
    }

    [Inject]
    BreadcrumbManager injectBreadcrumbManager
    {
        set
        {
            BreadcrumbManager = value;
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

    [Inject]
    ILogin injectLogin
    {
        set
        {
            RequestSetup = value.SetupHttpRequest;
        }
    }

    [Inject]
    IGameErrorHandler _errorHandler;

    public EventTracker(MonoBehaviour behaviour):base(behaviour)
    {
        _errorHandler.Setup(this);
    }

}