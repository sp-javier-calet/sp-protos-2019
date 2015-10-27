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

        const string ReleaseMessageKey = "gameloading.release_message_{0}";

        const float FakeLoginDuration = 2.0f;

        public ILogin Login;
        public Localization Localization;
        public IAppEvents AppEvents;
        public IAlertView AlertView;

        public bool Debug;

        [HideInInspector]
        public UIStackController Popups;

        [SerializeField]
        GameObject _progressContainer;

        [SerializeField]
        GameLoadingBarController _loadingBar;

        [SerializeField]
        int _releaseMessageAmount = 5;

        // seconds to end progress when real action is finished
        [SerializeField]
        float _speedUpTime = 0.5f;

        bool _paused = false;

        public bool Paused
        {
            get
            {
                return _paused;
            }

            set
            {
                _paused = value;
            }
        }

        private List<ILoadingOperation> _operations = new List<ILoadingOperation>();
        private int _finishedOperations = -1;
        private float _currentOperationDuration;
        private IAlertView _alert;

        private LoadingOperation _loginOperation;

        bool HasFinished(float progress)
        {
            return Math.Abs(progress - 1) < Mathf.Epsilon;
        }

        ILoadingOperation CurrentOperation
        {
            get
            {
                if(_finishedOperations >= 0 && _finishedOperations < _operations.Count)
                {
                    return _operations[_finishedOperations];
                }
                return null;
            }
        }

        bool HasFinishedCurrentOperation
        {
            get
            {
                var op = CurrentOperation;
                return op != null && HasFinished(op.Progress) && HasFinished(CurrentOperationProgress);
            }
        }

        float CurrentOperationProgress
        {
            get
            {
                var op = CurrentOperation;
                if(op == null)
                {
                    return 0.0f;
                }
                var expected = op.ExpectedDuration;
                var duration = Mathf.Min(_currentOperationDuration, expected);
                if(expected > 0)
                {
                    var opProgress = op.Progress;
                    if(HasFinished(opProgress))
                    {
                        return Mathf.Lerp(opProgress, 1.0f, 1.0f - (expected - duration) / _speedUpTime);
                    }
                    else
                    {   
                        return duration / expected;
                    }
                }
                else
                {
                    return op.Progress;
                }
            }
        }

        float Percent
        {
            get
            {
                return (_finishedOperations + CurrentOperationProgress) / _operations.Count;
            }
        }

        string Message
        {
            get
            {
                string msg = null;
                if(Debug)
                {
                    var op = CurrentOperation;
                    if(op != null)
                    {
                        msg = op.Message;
                    }
                }
                if(string.IsNullOrEmpty(msg))
                {
                    msg = ReleaseMessage;
                }
                return msg;
            }
        }

        string ReleaseMessage
        {
            get
            {
                if(_releaseMessageAmount <= 0)
                {
                    return null;
                }

                var str = string.Format(ReleaseMessageKey, (int)Mathf.Floor(_releaseMessageAmount * Percent)); 
                if(Localization != null)
                {
                    str = Localization.Get(str);
                }
                return str;
            }
        }

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

            #if !UNITY_EDITOR
            Debug = UnityEngine.Debug.isDebugBuild;
            #endif

            if(Localization == null)
            {
                Localization = Localization.Default;
            }
                
            if(Login != null)
            {
                Login.ErrorEvent += OnLoginError;
                _loginOperation = new LoadingOperation(FakeLoginDuration, DoLogin);
                RegisterOperation(_loginOperation);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _finishedOperations = -1;
        }

        public void RegisterOperation(ILoadingOperation operation)
        {
            _operations.Add(operation);
        }

        void Update()
        {
            var percent = Percent;
            var op = CurrentOperation;
            if(_finishedOperations < 0 || HasFinishedCurrentOperation )
            {
                if(op != null)
                {
                    OnOperationEnd(op);
                }
                _finishedOperations++;
                _currentOperationDuration = 0.0f;
                op = CurrentOperation;
                if(op != null)
                {
                    OnOperationStart(op);
                }
            }

            _currentOperationDuration += Time.smoothDeltaTime;
            percent = Percent;

            _loadingBar.Percent = percent;
            var msg = Message;
            if(_loadingBar.Message != msg)
            {
                _loadingBar.Message = Message;
                if(op != null)
                {
                    OnOperationChange(op);
                }
            }

            if(!_paused && HasFinished(percent))
            {
                OnAllOperationsLoaded();
            }
        }

        virtual protected void OnOperationChange(ILoadingOperation operation)
        {
            DebugLog("op " + (_finishedOperations + 1) + " "+operation.Progress.ToString("0.00")+": "+operation.Message);
        }

        virtual protected void OnOperationEnd(ILoadingOperation operation)
        {
            DebugLog("op " + (_finishedOperations + 1) + " end");
            operation.Start();
        }

        virtual protected void OnOperationStart(ILoadingOperation operation)
        {
            DebugLog("op " + (_finishedOperations + 1) + " start");
            operation.Start();
        }

        [System.Diagnostics.Conditional("DEBUG_SPGAMELOADING")]
        void DebugLog(string msg)
        {
            DebugUtils.Log(string.Format("GameLoadingController: {0}", msg));
        }

        void DoLogin()
        {
            _loginOperation.Message = "logging in...";
            if(_progressContainer != null)
            {
                _progressContainer.SetActive(true);
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
                RegisterOperation(auxOp);
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
                        auxOp.Finish();
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
            string msg = null;
            if(!Error.IsNullOrEmpty(err))//errors are handled on OnLoginError when ErrorEvent is dispatched
            {
                msg = "login finished with error";
                DebugLog(string.Format("Login End Error {0}", err));
            }
            else
            {
                msg = "login finished sucessfully";
            }
            _loginOperation.Finish(msg);
        }

        public bool AllOperationsLoaded
        {
            get
            {
                return !_operations.Exists(o => !HasFinished(o.Progress));
            }
        }
    }
}
