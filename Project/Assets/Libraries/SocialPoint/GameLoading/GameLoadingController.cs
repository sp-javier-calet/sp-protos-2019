using System;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Alert;
using SocialPoint.Attributes;
using SocialPoint.Crash;
using SocialPoint.GUI;
using SocialPoint.Locale;
using SocialPoint.Login;
using SocialPoint.Base;
using UnityEngine;

namespace SocialPoint.GameLoading
{
    /// <summary>
    /// Game loading controller.
    /// First screen for games, shows a loading bar and a background
    /// Tries to login and handle login errors
    /// 
    /// Extended classes can add LoadingOperations to the LoadingBarController
    /// Dispatches event AllOperationsLoaded when all loadingOperations are finished
    /// 
    /// Extended classes must listen to Login.NewUserEvent in order to parse the game data into a model
    /// </summary>
    public class GameLoadingController : UIViewController
    {
        const string UpgradeKey = "upgrade";
        const string UpgradeDef = "Upgrade";
        const string ForceUpgradeKey = "force_upgrade";
        const string ForceUpgradeDef = "Force Upgrade";
        const string SuggestedUpgradeKey = "suggested_upgrade";
        const string SuggestedUpgradeDef = "Suggested Upgrade";
        const string LaterKey = "later";
        const string LaterDef = "Later";
        const string RetryKey = "retry";
        const string RetryDef = "Retry";
        const string InvalidPrivilegeTokenKey = "invalid_privilege_token";
        const string InvalidPrivilegeTokenDef = "Invalid Privilege Token";
        const string ConnectionErrorKey = "connection_error";
        const string ConnectionErrorDef = "Connection Error";
        const string MaintenanceModeKey = "maintenance_mode";
        const string MaintenanceModeDef = "Maintenance Mode";
        const string MaintenanceMessageKey = "maintenance_message";
        const string MaintenanceMessageDef = "The game state has been corrupted and cannot recoverered automatically.\nPlease contact our support team or restart the game.";
        const string InvalidSecurityTokenKey = "invalid_security_token";
        const string InvalidSecurityTokenDef = "Invalid Security token";
        const string InvalidSecurityTokenMessageKey = "invalid_security_message";
        const string InvalidSecurityTokenMessageDef = "The game state has been corrupted and cannot recoverered automatically.\nPlease contact our support team or restart the game.";
        const string ContactKey = "contact";
        const string ContactDef = "Contact";
        const string RestartKey = "restart";
        const string RestartDef = "Restart";
        const string ResponseErrorKey = "response_error";
        const string ResponseErrorDef = "Response Error";
        const string ProgressLoginStartKey = "progress_login_start";
        const string ProgressLoginStartDef = "Logging into servers";
        const string ProgressLoginEndKey = "login_progress_end";
        const string ProgressLoginEndDef = "Logged in";
        const string ProgressSuggestedUpdateSkipKey = "progress_suggested_update_skip";
        const string ProgressSuggestedUpdateSkipDef = "Will update later";
        public ILogin Login;
        public PopupsController Popups;
        public Localization Localization;
        public ICrashReporter CrashReporter;
        public GameObject ProgressContainer;
        public LoadingBarController LoadingBar;
        public IAlertView AlertView;
        protected List<LoadingOperation> _operations = new List<LoadingOperation>();
        protected LoadingOperation _loginOperation;
        IAlertView _alert;

        protected virtual void AllOperationsLoaded()
        {
            DebugLog("all operations loaded");
        }

        override protected void OnLoad()
        {
            base.OnLoad();

            if(CrashReporter != null)
            {
                CrashReporter.Enable();
            }
        }

        override protected void OnAppeared()
        {
            base.OnAppeared();
            _operations = new List<LoadingOperation>();

            _loginOperation = new LoadingOperation();
            RegisterLoadingOperation(_loginOperation);
            StartCoroutine(CheckAllOperationsLoaded());

            Login.ErrorEvent += OnLoginError;
            DoLogin();
        }

        public void RegisterLoadingOperation(LoadingOperation operation)
        {
            if(_operations == null)
            {
                _operations = new List<LoadingOperation>();
            }
            _operations.Add(operation);
            operation.ProgressChangedEvent += OnProgressChanged;
        }

        public void OnProgressChanged(string message)
        {
            if(message != string.Empty)
            {
                DebugLog(message);
            }
            float progress = 0;
            _operations.ForEach(p => progress += p.progress);
            float percent = (progress / _operations.Count);
            LoadingBar.UpdateProgress(percent, message);
        }

        void DebugLog(string msg)
        {
            DebugUtils.Log(string.Format("GameLoadingController {0}", msg));
        }

        void DoLogin()
        {
            _loginOperation.UpdateProgress(0.1f, Localization.Get(ProgressLoginStartKey, ProgressLoginStartDef));
            if(ProgressContainer != null)
            {
                ProgressContainer.SetActive(true);
            }
            Login.Login(OnLoginEnd);
        }

        override protected void OnDisappearing()
        {
            Login.ErrorEvent -= OnLoginError;
            base.OnDisappearing();
        }

        virtual protected void Restart()
        {
            DoLogin();
        }

        void OnLoginUpgrade(GenericData data)
        {
            _alert = (IAlertView)AlertView.Clone();
            _alert.Message = data.Upgrade.Message;
            if(data.Upgrade.Type == UpgradeType.Forced)
            {
                _alert.Title = Localization.Get(ForceUpgradeKey, ForceUpgradeDef);
                _alert.Buttons = new string[]{ Localization.Get(UpgradeKey, UpgradeDef) };
                _alert.Show((int result) => {
                    Application.OpenURL(data.StoreUrl);
                });
            }
            else //suggested
            {
                //used to block loading process until player chooses what to do,
                //because suggested does not fire an error that OnLoginEnd can catch
                var auxOp = new LoadingOperation();
                RegisterLoadingOperation(auxOp);
                _alert.Title = Localization.Get(SuggestedUpgradeKey, SuggestedUpgradeDef);
                _alert.Buttons = new string[]{
                    Localization.Get(UpgradeKey, UpgradeDef),
                    Localization.Get(LaterKey, LaterDef)
                };
                _alert.Show((int result) => {
                    if(result == 0)
                    {
                        Application.OpenURL(data.StoreUrl);
                    }
                    else
                    {
                        auxOp.FinishProgress(Localization.Get(ProgressSuggestedUpdateSkipKey, ProgressSuggestedUpdateSkipDef));
                    }
                });
            }
        }

        void OnLoginError(ErrorType error, string msg, Attr data)
        {
            DebugLog(string.Format("Login Error {0} {1} {2}", error, msg, data));
            _alert = (IAlertView)AlertView.Clone();
            _alert.Signature = data.AsDic.GetValue(SocialPointLogin.AttrKeySignature).ToString();

            switch(error)
            {
            case ErrorType.Upgrade:
                OnLoginUpgrade(Login.Data);
                break;
            case ErrorType.MaintenanceMode:
                {
                    var popup = Popups.CreateChild<MaintenanceModePopupController>();
                    var title = Login.Data.Maintenance.Title;
                    if(string.IsNullOrEmpty(title))
                    {
                        title = Localization.Get(MaintenanceModeKey, MaintenanceModeDef);
                    }
                    string message = Login.Data.Maintenance.Message;
                    if(string.IsNullOrEmpty(message))
                    {
                        message = Localization.Get(MaintenanceMessageKey, MaintenanceMessageDef);
                    }
                    popup.TitleText = title;
                    popup.MessageText = message; 
                    popup.Signature = data.AsDic.GetValue(SocialPointLogin.AttrKeySignature).ToString();
                    Popups.Push(popup);
                }
                break;

            case ErrorType.InvalidPrivilegeToken:
                _alert.Title = Localization.Get(InvalidPrivilegeTokenKey, InvalidPrivilegeTokenDef);
                _alert.Buttons = new string[]{ Localization.Get(RetryKey, RetryDef) };
                _alert.Message = msg;
                _alert.Show(OnInvalidPrivilegeTokenAlert);
                break;

            case ErrorType.Connection: 
                _alert.Title = Localization.Get(ConnectionErrorKey, ConnectionErrorDef);
                _alert.Buttons = new string[]{ Localization.Get(RetryKey, RetryDef) };
                _alert.Message = msg;
                _alert.Show(OnLoginErrorAlert);
                break;

            case ErrorType.InvalidSecurityToken:
                {
                    var popup = Popups.CreateChild<InvalidSecurityTokenPopupController>();
                    popup.AlertView = AlertView;
                    popup.TitleText = Localization.Get(InvalidSecurityTokenKey, InvalidSecurityTokenDef);
                    popup.MessageText = Localization.Get(InvalidSecurityTokenMessageKey, InvalidSecurityTokenMessageDef);
                    popup.Localization = Localization;
                    popup.ContactButtonText = Localization.Get(ContactKey, ContactDef);
                    popup.RestartButtonText = Localization.Get(RestartKey, RestartDef);
                    popup.Restart = OnInvalidSecurityTokenRestart;
                    popup.Signature = data.AsDic.GetValue(SocialPointLogin.AttrKeySignature).ToString();
                    Popups.Push(popup);
                }
                break;

            default:
                _alert.Title = Localization.Get(ResponseErrorKey, ResponseErrorDef);
                _alert.Buttons = new string[]{ Localization.Get(RetryKey, RetryDef) };
                _alert.Message = msg;
                _alert.Show(OnLoginErrorAlert);
                break;
            }
        }

        void OnInvalidPrivilegeTokenAlert(int result)
        {
            Restart();
        }

        void OnInvalidSecurityTokenRestart()
        {
            Login.ClearStoredUser();
            Restart();
        }

        void OnLoginErrorAlert(int result)
        {
            Restart();
        }

        void OnLoginEnd(Error err)
        {
            if(ProgressContainer != null)
            {
                ProgressContainer.SetActive(false);
            }
            if(!Error.IsNullOrEmpty(err))//errors are handled on OnLoginError when ErrorEvent is dispatched
            {
                DebugLog(string.Format("Login End Error {0}", err));
            }
            else
            {
                _loginOperation.FinishProgress(Localization.Get(ProgressLoginEndKey, ProgressLoginEndDef));
            }
        }

        IEnumerator CheckAllOperationsLoaded()
        {
            while(_operations.Exists(o => o.progress < 1))
            {
                yield return null;
            }
            AllOperationsLoaded();
        }
    }
}

