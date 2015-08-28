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
        const string upgradeKey = "upgrade";
        const string forceUpgradeKey = "force_upgrade";
        const string suggestedUpgradeKey = "suggested_upgrade";
        const string laterKey = "later";
        const string retryKey = "retry";
        const string invalidPrivilegeTokenKey = "invalid_privilege_token";
        const string connectionErrorKey = "connection_error";
        const string maintenanceModeKey = "maintenance_mode";
        const string maintenanceMessageKey = "maintenance_message";
        const string invalidSecurityTokenKey = "invalid_security_token";
        const string invalidSecurityTokenMessageKey = "invalid_security_message";
        const string contactKey = "contact";
        const string restartKey = "restart";
        const string responseErrorKey = "response_error";

        public ILogin Login;
        public PopupsController Popups;
        public Localization Localization;
        public ICrashReporter CrashReporter;

        public GameObject ProgressContainer;
        public LoadingBarController LoadingBar;

        protected List<LoadingOperation> _operations = new List<LoadingOperation>();
        protected LoadingOperation _loginOperation;

        [HideInInspector]
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
            var alert = (IAlertView)AlertView.Clone();
            alert.Signature = data.AsDic.GetValue(SocialPointLogin.AttrKeySignature).ToString();
            var genericData = new LoginGenericData(data);
            string textButton0;
            string textButton1;

            switch(error)
            {
            case ErrorType.ForceUpgrade:
                if(genericData.Upgrade.Type == UpgradeType.Forced)
                {
                    alert.Title = new LocalizedString(forceUpgradeKey, "Force Upgrade", Localization);
                    ;
                    textButton0 = new LocalizedString(upgradeKey, "Upgrade", Localization);
                    alert.Buttons = new string[]{ textButton0 };
                    alert.Show((int result) => Application.OpenURL(genericData.StoreUrl)  
                    );
                }
                else //suggested
                {
                    //used to block loading process until player chooses what to do,
                    //because suggested does not fire an error that OnLoginEnd can catch
                    var auxOp = new LoadingOperation();
                    RegisterLoadingOperation(auxOp);
                    alert.Title = new LocalizedString(suggestedUpgradeKey, "Suggested Upgrade", Localization);
                    textButton0 = new LocalizedString(upgradeKey, "Upgrade", Localization);
                    textButton1 = new LocalizedString(laterKey, "Later", Localization);
                    alert.Buttons = new string[]{ textButton0, textButton1 };
                    alert.Show((int result) => {
                        if(result == 0)
                        {
                            Application.OpenURL(genericData.StoreUrl);
                        }
                        else
                        {
                            auxOp.FinishProgress("Will update later");
                        }
                    });
                }
                alert.Message = genericData.Upgrade.Message;
                break;

            case ErrorType.InvalidPrivilegeToken:
                alert.Title = new LocalizedString(invalidPrivilegeTokenKey, "Invalid Privilege Token", Localization);
                textButton0 = new LocalizedString(retryKey, "Retry", Localization);
                alert.Buttons = new string[]{ textButton0 };
                alert.Message = msg;
                alert.Show((int result) => DoLogin());
                break;

            case ErrorType.Connection: 
                alert.Title = new LocalizedString(connectionErrorKey, "Connection Error", Localization);
                textButton0 = new LocalizedString(retryKey, "Retry", Localization);
                alert.Buttons = new string[]{ textButton0 };
                alert.Message = msg;
                alert.Show((int result) => DoLogin());
                break;

            case ErrorType.MaintenanceMode:
                {
                    var popup = Popups.CreateChild<MaintenanceModePopupController>();
                    popup.TitleText = new LocalizedString(maintenanceModeKey, "Maintenance Mode", Localization);
                    popup.MessageText = new LocalizedString(maintenanceMessageKey, "The game state has been corrupted and cannot recoverered automatically.\nPlease contact our support team or restart the game.", Localization).ToString().Replace("\\n", "\n");
                    popup.Signature = data.AsDic.GetValue(SocialPointLogin.AttrKeySignature).ToString();
                    Popups.Push(popup);
                }
                break;

            case ErrorType.InvalidSecurityToken:
                {
                    var popup = Popups.CreateChild<InvalidSecurityTokenPopupController>();
                    popup.TitleText = new LocalizedString(invalidSecurityTokenKey, "Invalid Security token", Localization);
                    popup.MessageText = new LocalizedString(invalidSecurityTokenMessageKey, "The game state has been corrupted and cannot recoverered automatically.\nPlease contact our support team or restart the game.", Localization).ToString().Replace("\\n", "\n");

                    popup.ContactButtonText = new LocalizedString(contactKey, "Contact", Localization);
                    popup.RestartButtonText = new LocalizedString(restartKey, "Restart", Localization);
                    popup.Restart = () => {
                        Login.ClearUserId();
                        Application.LoadLevel(0);
                    };
                    popup.Signature = data.AsDic.GetValue(SocialPointLogin.AttrKeySignature).ToString();
                    Popups.Push(popup);
                }
                break;

            default:
                alert.Title = new LocalizedString(responseErrorKey, "Response Error", Localization);
                textButton0 = new LocalizedString(retryKey, "Retry", Localization);
                alert.Buttons = new string[]{ textButton0 };
                alert.Message = msg;
                alert.Show((int result) => {
                    Application.LoadLevel(0);
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
            if(!Error.IsNullOrEmpty(err))//errors are handled on OnLoginError when ErrorEvent is dispatched
            {
                DebugLog(string.Format("Login End Error {0}", err));
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

