using Zenject;
using SocialPoint.Network;
using SocialPoint.AppEvents;
using SocialPoint.Events;
using SocialPoint.Login;
using SocialPoint.ServerSync;
using SocialPoint.Alert;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Locale;
using UnityEngine;

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

    public CommandQueue(MonoBehaviour behaviour, IHttpClient client):base(behaviour, client)
    {
        AutoSync = OnAutoSync;
        GeneralError += OnGeneralError;
        CommandError += OnCommandError;
    }

    void OnGeneralError(CommandQueueErrorType type, Error err)
    {
        var signature = string.Format("{0}-{1}", (int) type, err.Code);
        ShowError(signature, err);
    }
    
    void OnCommandError(Command cmd, Error err, Attr resp)
    {
        var signature = string.Format("{0}-{1}", cmd.Id, err.Code);
        ShowError(signature, err);
    }
    
    void ShowError(string signature, Error err)
    {
        var alert = (IAlertView)_alertView.Clone();
        alert.Buttons = new string[]{ 
            _localization.Get("command_queue.general_error_popup_retry_button", "Retry")
        };
        alert.Title = _localization.Get("command_queue.general_error_popup_title", "Syncronization Error");
        var msg = _localization.Get(err);
        if(string.IsNullOrEmpty(msg))
        {
            msg = _localization.Get("command_queue.general_error_popup_message", "There was a problem trying to syncronize your game state with the server.");
        }
        alert.Message = msg;
        alert.Signature = signature;
        alert.Show((i) => {
            alert.Dispose();
            AppEvents.RestartGame();
        });
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