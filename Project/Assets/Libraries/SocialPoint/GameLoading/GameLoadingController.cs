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
        public ILogin Login;
        public PopupsController Popups;
        public Localization Localization;
        public ICrashReporter CrashReporter;

        public GameObject ProgressContainer;
        public LoadingBarController LoadingBar;

        protected List<LoadingOperation> _operations = new List<LoadingOperation>();
        protected LoadingOperation _loginOperation;

        public IAlertView AlertView;

        protected virtual void AllOperationsLoaded()
        {
            Debug.Log("all operations loaded");
        }

        override protected void OnLoad()
        {
            base.OnLoad();

            UnityAlertView.ShowDelegate = (GameObject go) => {
                var viewController = go.GetComponent<UIViewController>();
                Popups.Push(viewController);
            };
            UnityAlertView.HideDelegate = (GameObject go) => {
                var viewController = go.GetComponent<UIViewController>();
                viewController.Hide(true);
            };

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
                _operations = new List<LoadingOperation>();
            _operations.Add(operation);
            operation.ProgressChangedEvent += OnProgressChanged;
        }

        public void OnProgressChanged(string message)
        {
            if(message != string.Empty)
                Debug.Log(message);
            float progress = 0;
            _operations.ForEach(p => progress += p.progress);
            float percent = (progress / _operations.Count);
            LoadingBar.UpdateProgress(percent, message);
        }

        void DebugLog(string msg)
        {
            Debug.Log(msg);
        }

        void DoLogin()
        {
            _loginOperation.UpdateProgress(0.1f, "Logging into servers");
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

        void OnLoginError(ErrorType error, string msg, Attr data)
        {
            DebugLog(string.Format("Login Error {0} {1} {2}", error, msg, data));
            var alert = new AlertView();
            var genericData = new LoginGenericData(data);
            switch(error)
            {
            case ErrorType.ForceUpgrade:
                if(genericData.Upgrade.Type == UpgradeType.Forced)
                {
                    alert.Title = "Force Upgrade";
                    alert.Buttons =  new string[]{"UPGRADE"};
                    alert.Show((int result) => {
                        Application.OpenURL(genericData.StoreUrl);
                    }  
                    );
                }
                else //suggested
                {
                    alert.Title = "Suggested Upgrade";
                    alert.Buttons =  new string[]{"UPGRADE", "LATER"};
                    alert.Show((int result) => {
                        if(result == 0)
                        {
                            Application.OpenURL(genericData.StoreUrl);
                        }
                        else
                        {
                            Debug.Log("will upgrade later");
                        }
                    }  
                    );
                }
                alert.Message = genericData.Upgrade.Message;
                break;

            case ErrorType.InvalidPrivilegeToken:
                alert.Title = "Invalid Privilege Token";
                alert.Buttons = new string[]{ "RECONNECT" };
                alert.Message = msg;
                alert.Show((int result) => {
                    DoLogin();
                });
                break;

            case ErrorType.Connection:
                alert.Title = "Connection Error";
                alert.Buttons = new string[]{ "RECONNECT" };
                alert.Message = msg;
                alert.Show((int result) => {
                    DoLogin();
                });
                break;

            case ErrorType.MaintenanceMode:
                alert.Title = "Maintenance Mode";
                alert.Buttons = new string[]{ "RECONNECT" };
                alert.Message = msg;
                alert.Show((int result) => {
                    DoLogin();
                });
                break;

            case ErrorType.InvalidSecurityToken:
                break;
            default:
                alert.Buttons = new string[]{ "OK", "CANCEL", "LATER" };
                alert.Message = msg;
                alert.Show((int result) => {
                    Debug.Log(result);
                }  
                );
                break;
            }
        }

        void OnLoginEnd(Error err)
        {
            if(ProgressContainer != null)
            {
                ProgressContainer.SetActive(false);
            }
            if(!Error.IsNullOrEmpty(err))
            {
                DebugLog(string.Format("Login End Error {0}", err));
                /*
                var popup = Popups.CreateChild<GameLoadingErrorPopupController>();
                popup.Text = err.Msg;
                popup.Dismissed += OnErrorPopupDismissed;
                Popups.Push(popup);
                */
            }
            else
            {
                _loginOperation.FinishProgress("login ready");
            }
        }

        void OnErrorPopupDismissed()
        {
            DoLogin();
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

