using System;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Utils;
using SocialPoint.GUI;
using SocialPoint.Login;
using SocialPoint.Locale;
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

        public GameObject ProgressContainer;
        public LoadingBarController LoadingBar;

        protected List<LoadingOperation> _operations = new List<LoadingOperation>();
        protected LoadingOperation _loginOperation;

        public event Action AllOperationsLoaded;

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
            _operations.ForEach(p => progress += p.Progress);
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
                var popup = Popups.CreateChild<GameLoadingErrorPopupController>();
                popup.Text = err.Msg;
                popup.Dismissed += OnErrorPopupDismissed;
                Popups.Push(popup);
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
            while(_operations.Exists(o => o.Progress < 1))
            {
                yield return null;
            }
            AllOperationsLoaded();
        }
    }
}

