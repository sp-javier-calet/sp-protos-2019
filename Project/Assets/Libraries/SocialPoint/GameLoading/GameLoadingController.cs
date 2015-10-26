using System;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Alert;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.GUIControl;
using SocialPoint.Locale;
using SocialPoint.Login;
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
        const string InvalidPrivilegeTokenMessageKey = "gameloading.invalid_privilege_token_message";
        const string InvalidPrivilegeTokenMessageDef = "The game will restart without privilege token.";
        const string ConnectionErrorTitleKey = "gameloading.connection_error_title";
        const string ConnectionErrorTitleDef = "Connection Error";
        const string ConnectionErrorMessageKey = "gameloading.connection_error_message";
        const string ConnectionErrorMessageDef = "Could not reach the server. Please check your connection and try again.";
        const string MaintenanceModeTitleKey = "gameloading.maintenance_mode_title";
        const string MaintenanceModeTitleDef = "Maintenance Mode";
        const string MaintenanceModeMessageKey = "gameloading.maintenance_mode_message";
        const string MaintenanceModeMessageDef = "We are performing scheduled maintenance.\nWe should be back online shortly.";

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
        public IAppEvents AppEvents;
        public GameObject ProgressContainer;
        public GameLoadingBarController LoadingBar;
        public IAlertView AlertView;
        public bool Debug;

        bool _paused = false;

        public bool Paused
        {
            get
            {
                return _paused;
            }

            set
            {
                if(_paused != value)
                {
                    _paused = value;
                    if(!_paused && AllOperationsLoaded && AllOperationsFakedLoaded)
                    {
                        OnAllOperationsLoaded();
                    }
                }
            }
        }

        private List<LoadingOperation> _operations = new List<LoadingOperation>();
        private LoadingOperation _loginOperation;
        private IAlertView _alert;

        protected virtual void OnAllOperationsLoaded()
        {
            DebugLog("all operations loaded");

            if(AppEvents != null)
            {
                AppEvents.TriggerGameWasLoaded();
            }
        }

        override protected void OnLoad()
        {
            base.OnLoad();

            Debug = UnityEngine.Debug.isDebugBuild;

            if(Localization == null)
            {
                Localization = Localization.Default;
            }
        }

        override protected void OnAppeared()
        {
            base.OnAppeared();
            _operations = new List<LoadingOperation>();

            _loginOperation = new LoadingOperation(6);
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
        }

        void Update()
        {
            float progress = 0;
            _operations.ForEach(p => {
                p.Update(Time.deltaTime);
                progress += p.FakeProgress;
            });
            float percent = (progress / _operations.Count);
            LoadingBar.UpdateProgress(percent, "");
            if(Math.Abs(percent - 1) < Mathf.Epsilon)
            {
                UnityEngine.Debug.Log("fake done");
                OnAllOperationsLoaded();
            }
        }

        [System.Diagnostics.Conditional("DEBUG_SPGAMELOADING")]
        void DebugLog(string msg)
        {
            DebugUtils.Log(string.Format("GameLoadingController {0}", msg));
        }

        void DoLogin()
        {
            _loginOperation.UpdateProgress(0, Localization.Get(ProgressLoginStartKey, ProgressLoginStartDef));
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
                _alert.Buttons = new string[] {
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

        void OnLoginError(ErrorType type, Error err, Attr data)
        {
            DebugLog(string.Format("Login Error {0} {1} {2}", type, err, data));
            _alert = (IAlertView)AlertView.Clone();
            _alert.Signature = data.AsDic.GetValue(SocialPointLogin.AttrKeySignature).ToString();

            switch(type)
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
                _alert.Message = GetErrorMessage(err, InvalidPrivilegeTokenMessageKey, InvalidPrivilegeTokenMessageDef);
                _alert.Show(OnInvalidPrivilegeTokenAlert);
                break;

            case ErrorType.Connection: 
                _alert.Title = Localization.Get(ConnectionErrorTitleKey, ConnectionErrorTitleDef);
                _alert.Buttons = new string[]{ Localization.Get(RetryButtonKey, RetryButtonDef) };
                _alert.Message = GetErrorMessage(err, ConnectionErrorMessageKey, ConnectionErrorMessageDef);
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
                _alert.Message = GetErrorMessage(err, ResponseErrorMessageKey, ResponseErrorMessageDef);
                _alert.Show(OnLoginErrorAlert);
                break;
            }
        }

        string GetErrorMessage(Error err, string key, string def)
        {
            if(Debug)
            {
                return err.ToString();
            }
            var msg = Localization.Get(err);
            if(string.IsNullOrEmpty(msg))
            {
                msg = Localization.Get(key, def);
            }
            return msg;
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
            /*
            if(ProgressContainer != null)
            {
                ProgressContainer.SetActive(false);
            }
            */
            if(!Error.IsNullOrEmpty(err))//errors are handled on OnLoginError when ErrorEvent is dispatched
            {
                DebugLog(string.Format("Login End Error {0}", err));
            }
            else
            {
                _loginOperation.FinishProgress(Localization.Get(ProgressLoginEndKey, ProgressLoginEndDef));
            }
        }

        public bool AllOperationsLoaded
        {
            get
            {
                return !_operations.Exists(o => o.Progress < 1);
            }
        }

        public bool AllOperationsFakedLoaded
        {
            get
            {
                return !_operations.Exists(o => o.FakeProgress < 1);
            }
        }

        IEnumerator CheckAllOperationsLoaded()
        {
            while(!(AllOperationsLoaded && AllOperationsFakedLoaded))
            {
                yield return null;
            }
            if(!_paused)
            {
                OnAllOperationsLoaded();
            }
        }
    }
}

