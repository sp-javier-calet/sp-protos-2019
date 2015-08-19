﻿using Zenject;
using SocialPoint.Network;
using SocialPoint.AppEvents;
using SocialPoint.Events;
using SocialPoint.Login;
using UnityEngine;

class CommandQueue : SocialPoint.ServerSync.CommandQueue
{

    [InjectOptional("command_queue_ignore_responses")]
    public bool InjectIgnoreResponses
    {
        set
        {
            IgnoreResponses = value;
        }
    }

    [InjectOptional("command_queue_send_interval")]
    public int InjectSendInterval
    {
        set
        {
            SendInterval = value;
        }
    }

    [InjectOptional("command_queue_outofsync_interval")]
    public int InjectMaxOutOfSyncInterval
    {
        set
        {
            MaxOutOfSyncInterval = value;
        }
    }

    [InjectOptional("command_queue_timeout")]
    public float InjectTimeout
    {
        set
        {
            Timeout = value;
        }
    }

    [InjectOptional("command_queue_backoff_multiplier")]
    public float InjectBackoffMultiplier
    {
        set
        {
            BackoffMultiplier = value;
        }
    }

    [InjectOptional("command_queue_ping_enabled")]
    public bool InjectPingEnabled
    {
        set
        {
            PingEnabled = value;
        }
    }

    [Inject]
    public IAppEvents InjectAppEvents
    {
        set
        {
            AppEvents = value;
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

    [Inject]
    public ILogin InjectLogin
    {
        set
        {
            RequestSetup = value.SetupHttpRequest;
        }
    }

    public CommandQueue(MonoBehaviour behaviour, IHttpClient client):base(behaviour, client)
    {
    }
}