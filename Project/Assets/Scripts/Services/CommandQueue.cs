using Zenject;
using SocialPoint.Network;
using SocialPoint.AppEvents;
using SocialPoint.ServerEvents;
using SocialPoint.Login;
using SocialPoint.ServerSync;
using SocialPoint.Alert;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Utils;
using SocialPoint.Locale;
using SocialPoint.GameLoading;
using System;

class CommandQueue : SocialPoint.ServerSync.CommandQueue
{

    [InjectOptional("command_queue_ignore_responses")]
    bool injectIgnoreResponses
    {
        set
        {
            IgnoreResponses = value;
        }
    }

    [InjectOptional("command_queue_send_interval")]
    int injectSendInterval
    {
        set
        {
            SendInterval = value;
        }
    }

    [InjectOptional("command_queue_outofsync_interval")]
    int injectMaxOutOfSyncInterval
    {
        set
        {
            MaxOutOfSyncInterval = value;
        }
    }

    [InjectOptional("command_queue_timeout")]
    float injectTimeout
    {
        set
        {
            Timeout = value;
        }
    }

    [InjectOptional("command_queue_backoff_multiplier")]
    float injectBackoffMultiplier
    {
        set
        {
            BackoffMultiplier = value;
        }
    }

    [InjectOptional("command_queue_ping_enabled")]
    bool injectPingEnabled
    {
        set
        {
            PingEnabled = value;
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
    IEventTracker injectEventTracker
    {
        set
        {
            TrackEvent = value.TrackEvent;
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
    CommandReceiver injectCommandReceiver
    {
        set
        {
            CommandReceiver = value;
        }
    }

    [Inject]
    ISerializer<PlayerModel> _playerSerializer;

    [Inject]
    GameModel _gameModel;

    [Inject]
    IGameErrorHandler _errorHandler;

    [Inject]
    IGameLoader gameLoader
    {
        set
        {
            AutoSync = value.OnAutoSync;
        }
    }

    [Inject]
    ILogin _login;

    public CommandQueue(ICoroutineRunner runner, IHttpClient client) : base(runner, client)
    {
        _errorHandler.Setup(this);
    }
}
