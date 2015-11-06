using Zenject;
using SocialPoint.Network;
using SocialPoint.AppEvents;
using SocialPoint.ServerEvents;
using SocialPoint.Login;
using SocialPoint.ServerSync;
using SocialPoint.Alert;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Locale;
using UnityEngine;
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
    ISerializer<PlayerModel> _playerSerializer;
        
    [Inject]
    IAlertView _alertView;
    
    [Inject]
    Localization _localization;

    [Inject]
    GameModel _gameModel;

    [Inject("sync_error")]
    Action<string,Error> _syncError;

    public CommandQueue(MonoBehaviour behaviour, IHttpClient client):base(behaviour, client)
    {
        AutoSync = OnAutoSync;
        GeneralError += OnGeneralError;
        CommandError += OnCommandError;
    }

    void OnGeneralError(CommandQueueErrorType type, Error err)
    {
        Stop();
        if(_syncError != null)
        {
            _syncError("queue-"+(int)type, err);
        }
    }
    
    void OnCommandError(Command cmd, Error err, Attr resp)
    {
        Stop();
        if(_syncError != null)
        {
            _syncError("cmd-"+cmd.Id, err);
        }
    }

    public Attr OnAutoSync()
    {
        if(_gameModel == null || _gameModel.Player == null)
        {
            return null;
        }
        return _playerSerializer.Serialize(_gameModel.Player);
    }
}