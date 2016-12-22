using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEditor;

namespace SocialPoint.TransparentBundles
{
    public class TransparentBundleAPI
    {
        public class LoginOptions
        {
            // Wether or not we should ask the user for credentials if login fails.
            public bool AutoRetryLogin = true;
            // Callback for login success
            public Action<RequestReport> LoginOk = null;
            // Callback for login failure
            public Action<RequestReport> LoginFailed = null;
        }

        private static bool _isLogged = false;

        public const string SERVER_URL = "http://httpbin.org/post";

        [MenuItem("SocialPoint/Test Call")]
        public static void test()
        {
            CreateBundle(new CreateBundlesArgs(x => Debug.Log(x.ResponseRes.Response), x => Debug.Log(x.LoginCancelled)));
        }

        #region LOGIN
        /// <summary>
        /// Entry point for standalone Login
        /// </summary>
        /// <param name="loginOptions">Optional login options. defaults to autologin = true and no callbacks</param>
        public static void Login(LoginOptions loginOptions = null)
        {
            if(loginOptions == null)
            {
                loginOptions = new LoginOptions();
            }

            // Gets the previously stored login info
            var loginUser = EditorPrefs.GetString(LoginWindow.LOGIN_PREF_KEY);

            // If there is no info stored
            if(string.IsNullOrEmpty(loginUser))
            {
                if(loginOptions.AutoRetryLogin)
                {
                    // If the retry login option is enabled prompt the user for login credentials
                    LoginWindow.Open(() => Login(loginOptions), () => OnLoginCancelled(loginOptions));
                }
                else
                {
                    // Else, fires the login failed report.
                    loginOptions.LoginFailed(new RequestReport(true, false, "No username found"));
                }
            }
            else
            {
                // Create and configure the request
                HttpAsyncRequest asyncReq = new HttpAsyncRequest(SERVER_URL, HttpAsyncRequest.MethodType.POST, x => HandleLoginResponse(x, loginOptions));

                // Send the request
                asyncReq.Send();
            }
        }

        /// <summary>
        /// Handles the result of the login request to the server
        /// </summary>
        /// <param name="result">ResponseResult of the petition</param>
        /// <param name="loginOptions">Login options for this login process</param>
        private static void HandleLoginResponse(ResponseResult result, LoginOptions loginOptions)
        {
            if(result != null && result.Success)
            {
                _isLogged = true;
                if(loginOptions.LoginOk != null)
                {
                    // fires the login success callback.
                    loginOptions.LoginOk(new RequestReport(true, false));
                }
            }
            else
            {
                // if the request has failed and the autoretry is enabled
                if(loginOptions.AutoRetryLogin)
                {
                    // We open the login window with the encountered error giving the user the possibility to change the login info
                    LoginWindow.Open(() => Login(loginOptions), () => OnLoginCancelled(loginOptions), result.Message);
                }
                else
                {
                    // fires the failed callback
                    if(loginOptions.LoginFailed != null)
                    {
                        loginOptions.LoginFailed(new RequestReport(true, false, result));
                    }
                }
            }
        }

        /// <summary>
        /// In case the login dialog is closed by the user without logging.
        /// </summary>
        /// <param name="loginOptions">The login options for this login process</param>
        private static void OnLoginCancelled(LoginOptions loginOptions)
        {
            if(loginOptions.LoginFailed != null)
            {
                // Fires the provided failed callback with a report of login cancelled
                loginOptions.LoginFailed(new RequestReport(true, true, "Login cancelled by user"));
            }
        }
        #endregion

        #region PUBLIC_METHODS
        /// <summary>
        /// Entry point for CreateBundle request. It will trigger a login if not previously logged for this session and then send the request
        /// </summary>
        /// <param name="arguments">Arguments needed for this type of request</param>
        /// <param name="autoRetryLogin">Wether or not the login information should be asked for the user or not in case of failure</param>
        public static void CreateBundle(CreateBundlesArgs arguments, bool autoRetryLogin = true)
        {
            // Build up the Login Options with callbacks and options
            var options = new LoginOptions();

            options.AutoRetryLogin = autoRetryLogin;
            options.LoginOk = (report) =>
            {
                arguments.SetRequestReport(report);
                CreateBundleAction(arguments);
            };

            options.LoginFailed = (report) =>
            {
                arguments.SetRequestReport(report);
                arguments.OnFailedCallback(report);
            };

            // Triggers login process
            LoginAndExecuteAction(options);
        }
        #endregion

        #region PRIVATE_METHODS
        /// <summary>
        /// If the editor has never logged in, tries to login and then proceeds with the action
        /// </summary>
        /// <param name="loginOptions">The arguments for the login process that contains callbacks and login settings</param>
        private static void LoginAndExecuteAction(LoginOptions loginOptions)
        {
            if(!_isLogged)
            {
                Login(loginOptions);
            }
            else
            {
                loginOptions.LoginOk(new RequestReport(false, false));
            }
        }

        /// <summary>
        /// Single encapsulated action for creating a bundle request
        /// </summary>
        /// <param name="arguments">Create Bundle Request Arguments as RequestArgs generic class</param>
        private static void CreateBundleAction(RequestArgs arguments)
        {
            HttpAsyncRequest asyncReq = new HttpAsyncRequest(SERVER_URL, HttpAsyncRequest.MethodType.POST, arguments.UpdateReportAndCallback);

            asyncReq.Send();
        }

        #endregion
    }
}
