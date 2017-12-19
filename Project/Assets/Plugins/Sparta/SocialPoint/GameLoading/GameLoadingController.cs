using System;
using System.Collections.Generic;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Crash;
using SocialPoint.GUIControl;
using SocialPoint.Locale;
using SocialPoint.Login;
using SocialPoint.UIComponents;
using SocialPoint.Utils;
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
        public delegate void AllOperationsLoadedDelegate();

        AllOperationsLoadedDelegate AllOperationsLoadedEvent;

        const string ReleaseMessageKey = "gameloading.release_message_{0}";

        const float FakeLoginDuration = 2.0f;

        public ILogin Login;
        public Localization Localization;
        public IAppEvents AppEvents;
        public ICrashReporter CrashReporter;
        public IGameErrorHandler ErrorHandler;
        public INativeUtils NativeUtils;
        public bool Paused;

        [SerializeField]
        BasicProgressBarController _loadingProgressBar;

        [SerializeField]
        int _releaseMessageAmount = 5;

        int _currentRetriesToShowSupportButton = 0;
        const int RetriesToShowSupportButton = 3;

        // seconds to end progress when real action is finished
        [SerializeField]
        float _speedUpTime = 0.5f;

        List<ILoadingOperation> _operations = new List<ILoadingOperation>();
        int _currentOperationIndex = -1;
        float _currentOperationDuration;
        LoadingOperation _loginOperation;
        LoadingOperation _sendCrashesBeforeLoginOperation;

        static bool HasFinished(float progress)
        {
            return Math.Abs(progress - 1) < Mathf.Epsilon;
        }

        ILoadingOperation CurrentOperation
        {
            get
            {
                if(_currentOperationIndex >= 0 && _currentOperationIndex < _operations.Count)
                {
                    return _operations[_currentOperationIndex];
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
                    return duration / expected;
                }
                return op.Progress;
            }
        }

        float Progress
        {
            get
            {
                bool allOpsExpected = true;
                float totalExpected = 0.0f;
                float finishedExpected = 0.0f;
                int i = 0;
                for(int j = 0, _operationsCount = _operations.Count; j < _operationsCount; j++)
                {
                    var op = _operations[j];
                    if(!op.HasExpectedDuration)
                    {
                        allOpsExpected = false;
                        break;
                    }
                    else
                    {
                        var opExpected = op.ExpectedDuration;
                        if(i == _currentOperationIndex)
                        {
                            finishedExpected += CurrentOperationProgress * opExpected;
                        }
                        else if(i < _currentOperationIndex)
                        {
                            finishedExpected += opExpected;
                        }
                        totalExpected += opExpected;
                    }
                    i++;
                }

                if(allOpsExpected)
                {
                    return finishedExpected / totalExpected;
                }
                return (_currentOperationIndex + CurrentOperationProgress) / _operations.Count;
            }
        }

        string Message
        {
            get
            {
                string msg = null;
                if(ErrorHandler != null && ErrorHandler.Debug)
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

                var str = string.Format(ReleaseMessageKey, (int)Mathf.Floor(_releaseMessageAmount * Progress)); 
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
            AllOperationsLoadedEvent -= OnAllOperationsLoaded;
            if(AppEvents != null)
            {
                AppEvents.TriggerGameWasLoaded();
            }
        }

        override protected void OnLoad()
        {
            base.OnLoad();
                            
            DebugUtils.Assert(Login != null, "Login cannot be null");
            if(Login != null)
            {
                Login.ErrorEvent += OnLoginError;
                AllOperationsLoadedEvent += OnAllOperationsLoaded;
                _loginOperation = new LoadingOperation(FakeLoginDuration, DoLogin);
                RegisterOperation(_loginOperation);
            }

            DebugUtils.Assert(CrashReporter != null, "CrashReporter cannot be null");
            if(CrashReporter != null)
            {
                _sendCrashesBeforeLoginOperation = new LoadingOperation(FakeLoginDuration, DoSendCrashesBeforeLoginOperation);
                RegisterOperation(_sendCrashesBeforeLoginOperation);
            }

            DebugUtils.Assert(Localization != null, "Localization cannot be null");
            DebugUtils.Assert(ErrorHandler != null, "ErrorHandler cannot be null");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Restart();
        }

        public void RegisterOperation(ILoadingOperation operation)
        {
            if(_currentOperationIndex < 0)
            {
                _operations.Add(operation);
            }
            else
            {
                _operations.Insert(_currentOperationIndex + 1, operation);
            }
        }

        void Update()
        {
            float percent;
            var op = CurrentOperation;
            if(_currentOperationIndex < 0 || HasFinishedCurrentOperation)
            {
                if(op != null)
                {
                    OnOperationEnd(op);
                }
                _currentOperationIndex++;
                _currentOperationDuration = 0.0f;
                op = CurrentOperation;
                if(op != null)
                {
                    OnOperationStart(op);
                }
            }

            _currentOperationDuration += Time.smoothDeltaTime;
            percent = Progress;

            _loadingProgressBar.Percent = percent;
            var msg = Message;
            if(_loadingProgressBar.Message != msg)
            {
                _loadingProgressBar.Message = Message;
                if(op != null)
                {
                    OnOperationChange(op);
                }
            }

            if(!Paused && HasFinished(percent))
            {
                Paused = true;
                if(AllOperationsLoadedEvent != null)
                {
                    AllOperationsLoadedEvent();
                }
            }
        }

        virtual protected void OnOperationChange(ILoadingOperation operation)
        {
            DebugLog("op " + (_currentOperationIndex + 1) + " " + operation.Progress.ToString("0.00") + ": " + operation.Message);
        }

        virtual protected void OnOperationEnd(ILoadingOperation operation)
        {
            DebugLog("op " + (_currentOperationIndex + 1) + " end");
        }

        virtual protected void OnOperationStart(ILoadingOperation operation)
        {
            DebugLog("op " + (_currentOperationIndex + 1) + " start");
            operation.Start();
        }
            
        protected override void DebugLog(string msg)
        {
            ShowDebugLogMessage(string.Format("GameLoadingController: {0}", msg));
        }

        void DoLogin()
        {
            _loginOperation.Message = "logging in...";
            if(_loadingProgressBar != null)
            {
                _loadingProgressBar.gameObject.SetActive(true);
            }
            Login.Login(OnLoginEnd);
        }

        override protected void OnDisappearing()
        {
            Login.ErrorEvent -= OnLoginError;
            if(AllOperationsLoadedEvent != null)
            {
                AllOperationsLoadedEvent();
            }
            base.OnDisappearing();
        }

        virtual protected void Restart()
        {
            _currentOperationIndex = -1;
        }

        void OnLoginError(ErrorType type, Error err, Attr data)
        {
            DebugLog(string.Format("Login Error {0} {1} {2}", type, err, data));
            ErrorHandler.Signature = data.AsDic.GetValue(SocialPointLogin.AttrKeySignature).ToString();

            switch(type)
            {
            case ErrorType.Upgrade:
                ErrorHandler.ShowUpgrade(Login.Data.Upgrade, success => {
                    if(success)
                    {
                        OpenUpgrade();
                    }
                });
                break;
            case ErrorType.MaintenanceMode:
                ErrorHandler.ShowMaintenance(Login.Data.Maintenance, OnLoginErrorShown);
                break;
            case ErrorType.Connection:
                ErrorHandler.ShowConnection(err, OnLoginErrorShown);
                break;
            case ErrorType.InvalidSecurityToken:
                ErrorHandler.ShowInvalidSecurityToken(OnInvalidSecurityTokenShown);
                break;
            default:
                {
                    ErrorHandler.ShowLogin(err, OnLoginErrorShown, _currentRetriesToShowSupportButton >= RetriesToShowSupportButton);
                    _currentRetriesToShowSupportButton++;
                }
                break;
            }
        }

        void OpenUpgrade()
        {
            if(Login.Data != null && !string.IsNullOrEmpty(Login.Data.StoreUrl))
            {
                Application.OpenURL(Login.Data.StoreUrl);
            }
            else if(NativeUtils != null)
            {
                NativeUtils.OpenUpgrade();
            }
            else
            {
                throw new InvalidOperationException("Could not show upgrade");
            }
        }

        void OnInvalidSecurityTokenShown()
        {
            Restart();
        }

        void OnConnectionErrorShown()
        {
        }

        void OnLoginErrorShown()
        {
            Restart();
        }

        void OnLoginEnd(Error err)
        {
            string msg;
            if(!Error.IsNullOrEmpty(err))//errors are handled on OnLoginError when ErrorEvent is dispatched
            {
                msg = "login finished with error";
                DebugLog(string.Format("Login End Error {0}", err));
            }
            else
            {
                _currentRetriesToShowSupportButton = 0;
                msg = "login finished sucessfully";
            }
            if(Error.IsNullOrEmpty(err) && Login.Data != null)
            {
                if(Login.Data.Upgrade != null && Login.Data.Upgrade.Type == UpgradeType.Suggested)
                {
                    var op = new LoadingOperation(0.0f);
                    RegisterOperation(op);
                    op.Message = "suggesting upgrade...";
                    ErrorHandler.ShowUpgrade(Login.Data.Upgrade, success => {
                        if(success)
                        {
                            OpenUpgrade();
                        }
                        op.Finish();
                    });
                }
                else if(Login.Data.Maintenance != null)
                {
                    var op = new LoadingOperation(0.0f);
                    RegisterOperation(op);
                    op.Message = "showing maintenance message...";
                    ErrorHandler.ShowMaintenance(Login.Data.Maintenance, () => op.Finish());
                }
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

        void DoSendCrashesBeforeLoginOperation()
        {
            _sendCrashesBeforeLoginOperation.Message = "sending crashes before login...";
            if(_loadingProgressBar != null)
            {
                _loadingProgressBar.gameObject.SetActive(true);
            }

            CrashReporter.SendCrashesBeforeLogin(() => _sendCrashesBeforeLoginOperation.Finish());
        }
    }
}
