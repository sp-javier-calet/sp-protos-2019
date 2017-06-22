using System;
using SocialPoint.Alert;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.GUIControl;
using SocialPoint.Locale;
using SocialPoint.Login;
using SocialPoint.ServerEvents;
using SocialPoint.ServerSync;
using UnityEngine.SceneManagement;

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

        void ShowLogin(Error err, Action finished);

        void ShowLink(ILink link, LinkConfirmType type, Attr data, ConfirmBackLinkDelegate cbk);
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
            queue.GeneralError += (type, err) => {
                queue.Stop();
                if(handler != null)
                {
                    handler.Signature = GetSignature(CommandQueueGeneralErrorPrefix, (int)type);
                    handler.ShowSync(err);
                }
            };

            queue.CommandError += (cmd, err, resp) => {
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
            tracker.GeneralError += (type, err) => {
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
        const string SkipButtonDef = "Skip";

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

        const string ConfirmLinkTitleKey = "game_errors.confirm_link_title";
        const string ConfirmLinkTitleDef = "Confirm Link accounts";
        const string ConfirmLinkMessageKey = "game_errors.confirm_link_message";
        const string ConfirmLinkMessageDef = "Choose which account should be linked";
        const string ConfirmLinkButtonCancelKey = "game_errors.confirm_link_button_cancel";
        const string ConfirmLinkButtonCancelDef = "Cancel";
        const string ConfirmLinkButtonKeepKey = "game_errors.confirm_link_button_keep";
        const string ConfirmLinkButtonKeepDef = "Keep";
        const string ConfirmLinkButtonChangeKey = "game_errors.confirm_link_button_change";
        const string ConfirmLinkButtonChangeDef = "Change";

        readonly IAlertView _alert;
        readonly Localization _locale;
        readonly IAppEvents _appEvents;
        readonly int _restartScene;

        UIStackController _popups;
        Func<UIStackController> _findPopups;

        public bool Debug { get; set; }

        public string Signature { set; private get; }

        public GameErrorHandler(IAlertView alert, Localization locale, IAppEvents appEvents, Func<UIStackController> findPopups, int restartScene = 0)
        {
            _alert = alert;
            _locale = locale;
            _appEvents = appEvents;
            _restartScene = restartScene;
            _findPopups = findPopups;
            Debug = DebugUtils.IsDebugBuild;

            DebugUtils.Assert(_alert != null, "Alert can not be null");
            DebugUtils.Assert(_locale != null, "Locale can not be null");
            DebugUtils.Assert(_appEvents != null, "AppEvents can not be null");

            SceneManager.sceneLoaded += OnSceneLoaded;
            ReloadPopups();
        }

        public void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ReloadPopups();
        }

        void ReloadPopups()
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
                alert.Buttons = new []{ _locale.Get(UpgradeButtonKey, UpgradeButtonDef) };
                alert.Show(result => {
                    if(finished != null)
                    {
                        finished(true);
                    }
                });
            }
            else //suggested
            {
                alert.Title = _locale.Get(SuggestedUpgradeTitleKey, SuggestedUpgradeTitleDef);
                alert.Buttons = new [] {
                    _locale.Get(UpgradeButtonKey, UpgradeButtonDef),
                    _locale.Get(UpgradeLaterButtonKey, UpgradeLaterButtonDef)
                };
                alert.Show(result => {
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
                alert.Buttons = new []{ button };
                alert.Show(i => {
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
            alert.Buttons = Debug ? new[] {
                _locale.Get(RetryButtonKey, RetryButtonDef),
                SkipButtonDef
            } : new[] {
                _locale.Get(RetryButtonKey, RetryButtonDef)
            };
            alert.Message = GetErrorMessage(err, ConnectionErrorMessageKey, ConnectionErrorMessageDef);
            alert.Signature = Signature + "-" + err.Code;
            alert.Show(i => {
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
                popup.AlertView = _alert;
                _popups.Push(popup);
            }
        }

        public virtual void ShowLogin(Error err, Action finished)
        {
            var alert = (IAlertView)_alert.Clone();
            alert.Title = _locale.Get(ResponseErrorTitleKey, ResponseErrorTitleDef);
            alert.Buttons = new []{ _locale.Get(RetryButtonKey, RetryButtonDef) };
            alert.Message = GetErrorMessage(err, ResponseErrorMessageKey, ResponseErrorMessageDef);
            alert.Signature = Signature + "-" + err.Code;
            alert.Show(i => {
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
            alert.Buttons = new [] {
                _locale.Get(SyncButtonKey, SyncButtonDef)
            };
            alert.Show(i => _appEvents.RestartGame(_restartScene));
        }

        public virtual void ShowLink(ILink link, LinkConfirmType linkConfirmType, Attr data, ConfirmBackLinkDelegate cbk)
        {
            var alert = (IAlertView)_alert.Clone();
            alert.Title = _locale.Get(ConfirmLinkTitleKey, ConfirmLinkTitleDef);
            alert.Message = _locale.Get(ConfirmLinkMessageKey, ConfirmLinkMessageDef);
            alert.Signature = Signature;
            alert.Buttons = new [] {
                _locale.Get(ConfirmLinkButtonKeepKey, ConfirmLinkButtonKeepDef),
                _locale.Get(ConfirmLinkButtonChangeKey, ConfirmLinkButtonChangeDef),
                _locale.Get(ConfirmLinkButtonCancelKey, ConfirmLinkButtonCancelDef)
            };
            alert.Show(result => {
                if(cbk != null)
                {
                    cbk(result == 0 ? LinkConfirmDecision.Cancel : result == 1 ? LinkConfirmDecision.Keep : LinkConfirmDecision.Change);
                }
            });
        }
    }
}
