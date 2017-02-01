using System;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SocialPoint.TransparentBundles
{
    public static class TransparentBundleAPI
    {
        public class LoginOptions
        {
            /// <summary>
            /// Wether or not we should ask the user for credentials if login fails.
            /// </summary>
            public bool AutoRetryLogin = true;
            /// <summary>
            /// Callback for login success
            /// </summary>
            public Action<RequestReport> LoginOk = null;
            /// <summary>
            /// Callback for login failure
            /// </summary>
            public Action<RequestReport> LoginFailed = null;
            /// <summary>
            /// Logins with this username instead of the one stored in EditorPrefs
            /// </summary>
            public string OverwriteLoginUsername = "";
        }

        private const string _loginUrl = "https://transparentbundles.socialpoint.es/transparent_bundles/login/";
        private const string _requestUrl = "https://transparentbundles.socialpoint.es/transparent_bundles/asset_request/";
        private const string _localBundleUrl = "https://transparentbundles.socialpoint.es/transparent_bundles/local_asset/";

        private const string _queryLogin = "user_email";
        private const string _queryProject = "project";

        private const string _configDefaultPath = "Assets/Sparta/Config/TransparentBundles/TBConfig.asset";

        private static bool _isLogged = false;

        [MenuItem("SocialPoint/Test Call")]
        public static void test()
        {
            CreateBundle(new CreateBundlesArgs(x => Debug.Log(x.ResponseRes.Response), x => Debug.LogError(x.RequestCancelled)));
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
            var loginUser = string.IsNullOrEmpty(loginOptions.OverwriteLoginUsername) ? EditorPrefs.GetString(LoginWindow.LOGIN_PREF_KEY) : loginOptions.OverwriteLoginUsername;

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
                    loginOptions.LoginFailed(new RequestReport());
                }
            }
            else
            {
                // Create and configure the request
                HttpAsyncRequest asyncReq = new HttpAsyncRequest(HttpAsyncRequest.GetURLWithQuery(_loginUrl, GetBaseQueryArgs()), HttpAsyncRequest.MethodType.GET, x => HandleLoginResponse(x, loginOptions));

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
                    loginOptions.LoginOk(new RequestReport());
                }
            }
            else
            {
                _isLogged = false;
                // if the request has failed and the autoretry is enabled
                if(loginOptions.AutoRetryLogin)
                {
                    // We open the login window with the encountered error giving the user the possibility to change the login info
                    LoginWindow.Open(() => Login(loginOptions), () => OnLoginCancelled(loginOptions), result.Response);
                }
                else
                {
                    // fires the failed callback
                    if(loginOptions.LoginFailed != null)
                    {
                        loginOptions.LoginFailed(new RequestReport(result));
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
                loginOptions.LoginFailed(new RequestReport(true));
            }
        }

        private static string GetProject()
        {
            string project = string.Empty;
            var file = AssetDatabase.FindAssets("t:TBConfig");
            TBConfig config;

            if(file.Length == 0)
            {
                var directory = Directory.GetParent(_configDefaultPath);
                if(!directory.Exists)
                {
                    directory.Create();
                }

                config = ScriptableObject.CreateInstance<TBConfig>();

                AssetDatabase.CreateAsset(config, _configDefaultPath);

                Selection.activeObject = config;

                throw new Exception("There was no TBConfig file, one has been created at " + _configDefaultPath + " configure it please.");
            }
            else if(file.Length > 1)
            {
                throw new Exception("More than one config file found, please have only one.");
            }
            else
            {
                config = AssetDatabase.LoadAssetAtPath<TBConfig>(AssetDatabase.GUIDToAssetPath(file[0]));
            }

            project = config.project;

            if(string.IsNullOrEmpty(project))
            {
                Selection.activeObject = config;

                throw new Exception("Project config is empty, please configure it");
            }

            return project;
        }        

        private static Dictionary<string,string> GetBaseQueryArgs()
        {
            var queryVars = new Dictionary<string, string>();
            queryVars.Add(_queryLogin, EditorPrefs.GetString(LoginWindow.LOGIN_PREF_KEY));
            queryVars.Add(_queryProject, GetProject());

            return queryVars;
        }
        #endregion

        #region PUBLIC_METHODS


        /// <summary>
        /// Sends a CreateBundle request. It will trigger a login if not previously logged for this session and then sends the request
        /// </summary>
        /// <param name="arguments">Arguments needed for this type of request</param>
        /// <param name="autoRetryLogin">Wether or not the login information should be asked for the user or not in case of failure</param>
        public static void CreateBundle(CreateBundlesArgs arguments)
        {
            // Build up the Login Options with callbacks and options
            var options = new LoginOptions();

            options.AutoRetryLogin = arguments.AutoRetryLogin;
            options.LoginOk = (report) =>
            {
                var request = (HttpWebRequest)HttpWebRequest.Create(HttpAsyncRequest.GetURLWithQuery(_requestUrl, GetBaseQueryArgs()));
                request.Method = "POST";
                var requestData = new AsyncRequestData(request, x => HandleActionResponse(x, arguments, CreateBundle));
                arguments.SetRequestReport(report);
                ActionRequest(arguments, requestData);
            };

            options.LoginFailed = (report) =>
            {
                arguments.SetRequestReport(report);
                arguments.OnFailedCallback(report);
            };

            // Triggers login process
            LoginAndExecuteAction(options);
        }

        /// <summary>
        /// Sends a RemoveBundle request. It will trigger a login if not previously logged for this session and then sends the request
        /// </summary>
        /// <param name="arguments">Arguments needed for this type of request</param>
        /// <param name="autoRetryLogin">Wether or not the login information should be asked for the user or not in case of failure</param>
        public static void RemoveBundle(RemoveBundlesArgs arguments)
        {
            // Build up the Login Options with callbacks and options
            var options = new LoginOptions();

            options.AutoRetryLogin = arguments.AutoRetryLogin;
            options.LoginOk = (report) =>
            {
                var request = (HttpWebRequest)HttpWebRequest.Create(HttpAsyncRequest.GetURLWithQuery(_requestUrl, GetBaseQueryArgs()));
                request.Method = "DELETE";
                var requestData = new AsyncRequestData(request, x => HandleActionResponse(x, arguments, RemoveBundle));
                arguments.SetRequestReport(report);
                ActionRequest(arguments, requestData);
            };

            options.LoginFailed = (report) =>
            {
                arguments.SetRequestReport(report);
                arguments.OnFailedCallback(report);
            };

            // Triggers login process
            LoginAndExecuteAction(options);
        }


        /// <summary>
        /// Sends a MakeLocalBundle request. It will trigger a login if not previously logged for this session and then sends the request
        /// </summary>
        /// <param name="arguments">Arguments needed for this type of request</param>
        /// <param name="autoRetryLogin">Wether or not the login information should be asked for the user or not in case of failure</param>
        public static void MakeLocalBundle(MakeLocalBundlesArgs arguments)
        {
            // Build up the Login Options with callbacks and options
            var options = new LoginOptions();

            options.AutoRetryLogin = arguments.AutoRetryLogin;
            options.LoginOk = (report) =>
            {
                var request = (HttpWebRequest)HttpWebRequest.Create(HttpAsyncRequest.GetURLWithQuery(_localBundleUrl, GetBaseQueryArgs()));
                request.Method = "POST";
                var requestData = new AsyncRequestData(request, x => HandleActionResponse(x, arguments, MakeLocalBundle));
                arguments.SetRequestReport(report);
                ActionRequest(arguments, requestData);
            };

            options.LoginFailed = (report) =>
            {
                arguments.SetRequestReport(report);
                arguments.OnFailedCallback(report);
            };

            // Triggers login process
            LoginAndExecuteAction(options);
        }

        /// <summary>
        /// Sends a RemoveLocalBundle request. It will trigger a login if not previously logged for this session and then sends the request
        /// </summary>
        /// <param name="arguments">Arguments needed for this type of request</param>
        /// <param name="autoRetryLogin">Wether or not the login information should be asked for the user or not in case of failure</param>
        public static void RemoveLocalBundle(RemoveLocalBundlesArgs arguments)
        {
            // Build up the Login Options with callbacks and options
            var options = new LoginOptions();

            options.AutoRetryLogin = arguments.AutoRetryLogin;
            options.LoginOk = (report) =>
            {
                var request = (HttpWebRequest)HttpWebRequest.Create(HttpAsyncRequest.GetURLWithQuery(_localBundleUrl, GetBaseQueryArgs()));
                request.Method = "DELETE";
                var requestData = new AsyncRequestData(request, x => HandleActionResponse(x, arguments, RemoveLocalBundle));
                arguments.SetRequestReport(report);
                ActionRequest(arguments, requestData);
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
            if(!_isLogged || !string.IsNullOrEmpty(loginOptions.OverwriteLoginUsername))
            {
                Login(loginOptions);
            }
            else
            {
                loginOptions.LoginOk(new RequestReport());
            }
        }

        /// <summary>
        /// Single encapsulated action for creating a bundle request
        /// </summary>
        /// <param name="arguments">Create Bundle Request Arguments as RequestArgs generic class</param>
        private static void ActionRequest<T>(T arguments, AsyncRequestData requestData) where T : RequestArgs
        {
            HttpAsyncRequest asyncReq = new HttpAsyncRequest(requestData);

            asyncReq.Send();
        }

        private static void HandleActionResponse<T>(ResponseResult responseResult, T arguments, Action<T> retryRequest) where T : RequestArgs
        {
            if(responseResult.StatusCode == HttpStatusCode.Forbidden)
            {
                _isLogged = false;

                if(arguments.AutoRetryLogin)
                {
                    retryRequest(arguments);
                }
            }
            else
            {
                arguments.UpdateReportAndCallback(responseResult);
            }
        }

        #endregion
    }
}
