
using SocialPoint.Base;
using SocialPoint.Alert;
using SocialPoint.Login;
using SocialPoint.Locale;
using SocialPoint.AppEvents;
using SocialPoint.GUIControl;
using SocialPoint.Attributes;
using SocialPoint.ServerSync;
using SocialPoint.ServerEvents;
using UnityEngine;
using System;

namespace SocialPoint.GameLoading
{
    public interface IGameErrorHandler : IDisposable
    {
        string Signature { set; }
        bool Debug { get; set; }
        void ShowUpgrade(UpgradeData data, Action<bool> finished);
        void ShowSync(Error err);
        void ShowMaintenance(MaintenanceData data, Action finished);
        void ShowConnection(Error err, Action finished);
        void ShowInvalidSecurityToken(Action restart);
        void ShowLogin( Error err, Action finished);
    }

    public static class GameErrorHandlerExtensions
    {
        const string SignatureFormat = "{0}-{1}";
        const string CommandQueueGeneralErrorPrefix = "queue";
        const string CommandQueueCommandErrorPrefix = "cmd";
        const string EventTrackerGeneralErrorPrefix = "track";

        public static string GetSignature(string prefix, object obj)
        {
            return string.Format(SignatureFormat, prefix, obj);
        }

        public static void Setup(this IGameErrorHandler handler, ICommandQueue queue)
        {
            queue.GeneralError += (CommandQueueErrorType type, Error err) => {
                queue.Stop();
                if(handler != null)
                {
                    handler.Signature = GetSignature(CommandQueueGeneralErrorPrefix, (int)type);
                    handler.ShowSync(err);
                }
            };

            queue.CommandError += (Command cmd, Error err, Attr resp) => {
                queue.Stop();
                if(handler != null)
                {
                    handler.Signature = GetSignature(CommandQueueCommandErrorPrefix, cmd.Id);
                    handler.ShowSync(err);
                }
            };
        }

        public static void Setup(this IGameErrorHandler handler, IEventTracker tracker)
        {
            tracker.GeneralError += (EventTrackerErrorType type, Error err) => {
                if(type == EventTrackerErrorType.SessionLost)
                {
                    tracker.Stop();
                    if(handler != null)
                    {
                        handler.Signature = GetSignature(EventTrackerGeneralErrorPrefix, (int)type);
                        handler.ShowSync(err);
                    }
                }
            };
        }
    }

    public class GameErrorHandler : IGameErrorHandler
    {
        const string RetryButtonKey = "game_errors.retry_button";
        const string RetryButtonDef = "Retry";

        const string UpgradeButtonKey = "game_errors.upgrade_button";
        const string UpgradeButtonDef = "Upgrade";
        const string ForceUpgradeTitleKey = "game_errors.force_upgrade_title";
        const string ForceUpgradeTitleDef = "Force Upgrade";
        const string SuggestedUpgradeTitleKey = "game_errors.suggested_upgrade_title";
        const string SuggestedUpgradeTitleDef = "Suggested Upgrade";
        const string UpgradeLaterButtonKey = "game_errors.upgrade_later_button";
        const string UpgradeLaterButtonDef = "Later";

        const string SyncTitleKey = "game_errors.sync_title";
        const string SyncTitleDef = "Syncronization Error";
        const string SyncMessageKey = "game_errors.sync_message";
        const string SyncMessageDef = "There was a problem syncronizing the game state with the server. The game will be restarted.";
        const string SyncButtonKey = "game_errors.sync_button";
        const string SyncButtonDef = "Ok";

        const string MaintenanceModeTitleKey = "game_errors.maintenance_mode_title";
        const string MaintenanceModeTitleDef = "Maintenance Mode";
        const string MaintenanceModeMessageKey = "game_errors.maintenance_mode_message";
        const string MaintenanceModeMessageDef = "We are performing scheduled maintenance.\nWe should be back online shortly.";
        const string MaintenanceModeButtonKey = "game_errors.maintenance_mode_button";
        const string MaintenanceModeButtonDef = "Ok";

        const string ConnectionErrorTitleKey = "game_errors.connection_error_title";
        const string ConnectionErrorTitleDef = "Connection Error";
        const string ConnectionErrorMessageKey = "game_errors.connection_error_message";
        const string ConnectionErrorMessageDef = "Could not reach the server. Please check your connection and try again.";
        
        const string ResponseErrorTitleKey = "gameloading.response_error_title";
        const string ResponseErrorTitleDef = "Login Error";
        const string ResponseErrorMessageKey = "gameloading.response_error_message";
        const string ResponseErrorMessageDef = "There was an unknown error logging in. Please try again later.";

        IAlertView _alert;
        Localization _locale;
        IAppEvents _appEvents;
        UIStackController _popups;
        Func<UIStackController> _findPopups;

        public bool Debug { get; set; }

        public string Signature { set; private get; }

        public GameErrorHandler(IAlertView alert=null, Localization locale=null, IAppEvents appEvents=null, Func<UIStackController> findPopups=null)
        {
            _alert = alert;
            _locale = locale;
            _appEvents = appEvents;
            _findPopups = findPopups;
            Debug = UnityEngine.Debug.isDebugBuild;

            if(_alert == null)
            {
                _alert = new AlertView();
            }
            if(_locale == null)
            {
                _locale = Localization.Default;
            }
            if(_appEvents == null)
            {
                _appEvents = new SocialPointAppEvents();
            }

            _appEvents.LevelWasLoaded += OnLevelWasLoaded;
            OnLevelWasLoaded(Application.loadedLevel);
        }

        public void Dispose()
        {
            _appEvents.LevelWasLoaded -= OnLevelWasLoaded;
        }

        void OnLevelWasLoaded(int i)
        {
            _popups = null;
            if(_findPopups != null)
            {
                _popups = _findPopups();
            }
        }
                
        string GetErrorMessage(Error err, string key, string def)
        {
            if(Debug)
            {
                return err.ToString();
            }
            var msg = _locale.Get(err);
            if(string.IsNullOrEmpty(msg))
            {
                msg = _locale.Get(key, def);
            }
            return msg;
        }

        public virtual void ShowUpgrade(UpgradeData data, Action<bool> finished)
        {
            var alert = (IAlertView)_alert.Clone();
            alert.Message = data.Message;
            alert.Signature = Signature;
            if(data.Type == UpgradeType.Forced)
            {
                alert.Title = _locale.Get(ForceUpgradeTitleKey, ForceUpgradeTitleDef);
                alert.Buttons = new string[]{ _locale.Get(UpgradeButtonKey, UpgradeButtonDef) };
                alert.Show((int result) => {
                    if(finished != null)
                    {
                        finished(true);
                    }
                });
            }
            else //suggested
            {
                alert.Title = _locale.Get(SuggestedUpgradeTitleKey, SuggestedUpgradeTitleDef);
                alert.Buttons = new string[] {
                    _locale.Get(UpgradeButtonKey, UpgradeButtonDef),
                    _locale.Get(UpgradeLaterButtonKey, UpgradeLaterButtonDef)
                };
                alert.Show((int result) => {
                    bool success = result == 0;
                    if(finished != null)
                    {
                        finished(success);
                    }
                });
            }
        }

        public virtual void ShowMaintenance(MaintenanceData data, Action finished)
        {
            string title = null;
            string message = null;
            string button = null;
            if(data != null)
            {
                title = data.Title;
                message = data.Message;
                button = data.Button;
            }
            if(string.IsNullOrEmpty(title))
            {
                title = _locale.Get(MaintenanceModeTitleKey, MaintenanceModeTitleDef);
            }
            if(string.IsNullOrEmpty(message))
            {
                message = _locale.Get(MaintenanceModeMessageKey, MaintenanceModeMessageDef);
            }
            if(string.IsNullOrEmpty(button))
            {
                button = _locale.Get(MaintenanceModeButtonKey, MaintenanceModeButtonDef);
            }
            if(_popups != null)
            {
                var popup = _popups.CreateChild<MaintenanceModePopupController>();
                popup.TitleText = title;
                popup.MessageText = message;
                popup.ButtonText = button;
                popup.Signature = Signature;
                popup.Dismissed = finished;
                _popups.Push(popup);
            }
            else
            {
                var alert = (IAlertView)_alert.Clone();
                alert.Title = title;
                alert.Message = message;
                alert.Signature = Signature;
                alert.Buttons = new string[]{ button };
                alert.Show((i) => {
                    if(finished != null)
                    {
                        finished();
                    }
                });
            }
        }

        public virtual void ShowConnection(Error err, Action finished)
        {
            var alert = (IAlertView)_alert.Clone();
            alert.Title = _locale.Get(ConnectionErrorTitleKey, ConnectionErrorTitleDef);
            alert.Buttons = new string[]{ _locale.Get(RetryButtonKey, RetryButtonDef) };
            alert.Message = GetErrorMessage(err, ConnectionErrorMessageKey, ConnectionErrorMessageDef);
            alert.Signature = Signature + "-" + err.Code;
            alert.Show((i) => {
                if(finished != null)
                {
                    finished();
                }
            });
        }

        public virtual void ShowInvalidSecurityToken(Action restart)
        {
            if(_popups != null)
            {
                var popup = _popups.CreateChild<InvalidSecurityTokenPopupController>();
                popup.Localization = _locale;
                popup.Restart = restart;
                popup.Signature = Signature;
                _popups.Push(popup);
            }
            else
            {
            }
        }

        public virtual void ShowLogin(Error err, Action finished)
        {
            var alert = (IAlertView)_alert.Clone();
            alert.Title = _locale.Get(ResponseErrorTitleKey, ResponseErrorTitleDef);
            alert.Buttons = new string[]{ _locale.Get(RetryButtonKey, RetryButtonDef) };
            alert.Message = GetErrorMessage(err, ResponseErrorMessageKey, ResponseErrorMessageDef);
            alert.Signature = Signature + "-" + err.Code;
            alert.Show((i) => {
                if(finished != null)
                {
                    finished();
                }
            });
        }

        public virtual void ShowSync(Error err)
        {
            var alert = (IAlertView)_alert.Clone();
            alert.Title = _locale.Get(SyncTitleKey, SyncTitleDef);
            alert.Message = _locale.Get(SyncMessageKey, SyncMessageDef);
            alert.Signature = Signature + "-" + err.Code;
            alert.Buttons = new string[]{
                _locale.Get(SyncButtonKey, SyncButtonDef)
            };
            alert.Show((i) => {
                _appEvents.RestartGame();
            });
        }
    }
}
