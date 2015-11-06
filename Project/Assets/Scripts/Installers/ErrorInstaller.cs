using System;
using Zenject;
using SocialPoint.Locale;
using SocialPoint.AppEvents;
using SocialPoint.Alert;
using SocialPoint.Base;

public class ErrorInstaller : MonoInstaller
{
    [Serializable]
    public class SettingsData
    {

    }

    public SettingsData Settings;

    [Inject]
    Localization _localization;

    [Inject]
    IAlertView _alertView;

    [Inject]
    IAppEvents _appEvents;

    public override void InstallBindings()
    {

    }

    void ShowSyncError(string signature, Error err)
    {
        signature += "-" + err.Code;
        var alert = (IAlertView)_alertView.Clone();
        alert.Buttons = new string[]{ 
            _localization.Get("errors.sync_error_popup_retry_button", "Retry")
        };
        alert.Title = _localization.Get("errors.sync_error_popup_title", "Syncronization Error");
        var msg = _localization.Get(err);
        if(string.IsNullOrEmpty(msg))
        {
            msg = _localization.Get("errors.sync_error_popup_message", "There was a problem trying to syncronize your game state with the server.");
        }
        alert.Message = msg;
        alert.Signature = signature;
        alert.Show((i) => {
            alert.Dispose();
            _appEvents.RestartGame();
        });
    }
}


