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
        const string UpgradeButtonKey = "gameloading.upgrade_button";
        const string UpgradeButtonDef = "Upgrade";
        const string ForceUpgradeTitleKey = "gameloading.force_upgrade_title";
        const string ForceUpgradeTitleDef = "Force Upgrade";
        const string SuggestedUpgradeTitleKey = "gameloading.suggested_upgrade_title";
        const string SuggestedUpgradeTitleDef = "Suggested Upgrade";
        const string UpgradeLaterButtonKey = "gameloading.upgrade_later_button";
        const string UpgradeLaterButtonDef = "Later";
        const string RetryButtonKey = "gameloading.retry_button";
        const string RetryButtonDef = "Retry";
        const string InvalidPrivilegeTokenTitleKey = "gameloading.invalid_privilege_token_title";
        const string InvalidPrivilegeTokenTitleDef = "Invalid Privilege Token";
        const string ConnectionErrorTitleKey = "gameloading.connection_error_title";
        const string ConnectionErrorTitleDef = "Connection Error";
        const string MaintenanceModeTitleKey = "gameloading.maintenance_mode_title";
        const string MaintenanceModeTitleDef = "Maintenance Mode";
        const string MaintenanceModeMessageKey = "gameloading.maintenance_mode_message";
        const string MaintenanceModeMessageDef = "The game state has been corrupted and cannot recoverered automatically.\nPlease contact our support team or restart the game.";

        const string ResponseErrorTitleKey = "gameloading.response_error_title";
        const string ResponseErrorTitleDef = "Login Error";
        const string ResponseErrorMessageKey = "gameloading.response_error_message";
        const string ResponseErrorMessageDef = "There was an unknown error logging in. Please try again later.";
        const string ProgressLoginStartKey = "gameloading.progress_login_start";
        const string ProgressLoginStartDef = "Logging into servers";
        const string ProgressLoginEndKey = "gameloading.login_progress_end";
        const string ProgressLoginEndDef = "Logged in";
        const string ProgressSuggestedUpdateSkipKey = "gameloading.progress_suggested_update_skip";
        const string ProgressSuggestedUpdateSkipDef = "Will update later";

        public ILogin Login;
        public PopupsController Popups;
        public Localization Localization;
        public ILocalizationManager LocalizationManager;
        public ICrashReporter CrashReporter;
        public GameObject ProgressContainer;
        public LoadingBarController LoadingBar;
        public IAlertView AlertView;

        private List<LoadingOperation> _operations = new List<LoadingOperation>();
        private LoadingOperation _loginOperation;
        private IAlertView _alert;

        protected virtual void AllOperationsLoaded()
        {
            DebugLog("all operations loaded");
            if(LocalizationManager != null)
            {
                LocalizationManager.Load();
            }
        }

        override protected void OnLoad()
        {
            base.OnLoad();

            if(Localization == null && LocalizationManager != null)
            {
                Localization = LocalizationManager.Localization;
            }

            if(Localization == null)
            {
                Localization = Localization.Default;
            }

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

            if(Login != null)
            {
                Login.ErrorEvent += OnLoginError;
                DoLogin();
            }
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
            if(!string.IsNullOrEmpty(message))
            {
                DebugLog(message);
            }
            float progress = 0;
            _operations.ForEach(p => progress += p.progress);
            float percent = (progress / _operations.Count);
            LoadingBar.UpdateProgress(percent, message);
        }

        [System.Diagnostics.Conditional("DEBUG_SPGAMELOADING")]
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
                _alert.Title = Localization.Get(ForceUpgradeTitleKey, ForceUpgradeTitleDef);
                _alert.Buttons = new string[]{ Localization.Get(UpgradeButtonKey, UpgradeButtonDef) };
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
                _alert.Title = Localization.Get(SuggestedUpgradeTitleKey, SuggestedUpgradeTitleDef);
                _alert.Buttons = new string[]{
                    Localization.Get(UpgradeButtonKey, UpgradeButtonDef),
                    Localization.Get(UpgradeLaterButtonKey, UpgradeLaterButtonDef)
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
                    string title = null;
                    string message = null;
                    if(Login.Data != null && Login.Data.Maintenance != null)
                    {
                        title = Login.Data.Maintenance.Title;
                        message = Login.Data.Maintenance.Message;
                    }
                    if(string.IsNullOrEmpty(title))
                    {
                        title = Localization.Get(MaintenanceModeTitleKey, MaintenanceModeTitleDef);
                    }
                    if(string.IsNullOrEmpty(message))
                    {
                        message = Localization.Get(MaintenanceModeMessageKey, MaintenanceModeMessageDef);
                    }
                    popup.TitleText = title;
                    popup.MessageText = message; 
                    popup.Signature = data.AsDic.GetValue(SocialPointLogin.AttrKeySignature).ToString();
                    Popups.Push(popup);
                }
                break;

            case ErrorType.InvalidPrivilegeToken:
                _alert.Title = Localization.Get(InvalidPrivilegeTokenTitleKey, InvalidPrivilegeTokenTitleDef);
                _alert.Buttons = new string[]{ Localization.Get(RetryButtonKey, RetryButtonDef) };
                _alert.Message = msg;
                _alert.Show(OnInvalidPrivilegeTokenAlert);
                break;

            case ErrorType.Connection: 
                _alert.Title = Localization.Get(ConnectionErrorTitleKey, ConnectionErrorTitleDef);
                _alert.Buttons = new string[]{ Localization.Get(RetryButtonKey, RetryButtonDef) };
                _alert.Message = msg;
                _alert.Show(OnLoginErrorAlert);
                break;

            case ErrorType.InvalidSecurityToken:
                {
                    var popup = Popups.CreateChild<InvalidSecurityTokenPopupController>();
                    popup.Localization = Localization;
                    popup.Restart = OnInvalidSecurityTokenRestart;
                    popup.Signature = data.AsDic.GetValue(SocialPointLogin.AttrKeySignature).ToString();
                    Popups.Push(popup);
                }
                break;

            default:
                _alert.Title = Localization.Get(ResponseErrorTitleKey, ResponseErrorTitleDef);
                _alert.Buttons = new string[]{ Localization.Get(RetryButtonKey, RetryButtonDef) };
                if(!Debug.isDebugBuild)
                {
                    Localization.Get(ResponseErrorMessageKey, ResponseErrorMessageDef);
                }
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

