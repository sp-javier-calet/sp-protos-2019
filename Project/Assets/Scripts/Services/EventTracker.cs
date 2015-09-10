﻿using Zenject;
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

    [InjectOptional]
    BreadcrumbManager injectBreadcrumbManager
    {
        set
        {
            BreadcrumbManager = value;
        }
    }
    
    [Inject]
    ILogin injectLogin
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