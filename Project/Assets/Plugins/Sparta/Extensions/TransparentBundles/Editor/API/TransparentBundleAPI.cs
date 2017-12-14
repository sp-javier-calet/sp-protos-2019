using System;
using System.Collections.Generic;
using System.Net;
using LitJson;
using UnityEditor;

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
            public Action<RequestReport> LoginOk;
            /// <summary>
            /// Callback for login failure
            /// </summary>
            public Action<RequestReport> LoginFailed;
            /// <summary>
            /// Logins with this username instead of the one stored in EditorPrefs
            /// </summary>
            public string OverwriteLoginUsername = "";
        }

#if TB_DEBUG
        const string _hostName = "https://transparentbundles-pre.socialpoint.es";
#else
        const string _hostName = "https://transparentbundles.socialpoint.es";
#endif            
        const string _loginUrl = _hostName + "/transparent_bundles/login/";
        const string _requestUrl = _hostName + "/transparent_bundles/asset_request/";
        const string _removeUrl = _hostName + "/transparent_bundles/remove_asset_request/";
        const string _localBundleUrl = _hostName + "/transparent_bundles/local_asset/";
        const string _removeLocalBundleUrl = _hostName + "/transparent_bundles/remove_local_asset/";
        const string _cancelUrl = _hostName + "/transparent_bundles/cancel_request/";

        const string _queryLogin = "user_email";
        const string _queryProject = "project";
        const string _queryGuids = "asset_guids";

        static bool _isLogged;

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
            if(string.IsNullOrEmpty(loginUser) || string.IsNullOrEmpty(TBConfig.GetConfig().project))
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
                var asyncReq = new HttpAsyncRequest(HttpAsyncRequest.AppendQueryParams(_loginUrl, GetBaseQueryArgs()), HttpAsyncRequest.MethodType.GET, x => HandleLoginResponse(x, loginOptions));

                // Send the request
                asyncReq.Send();
            }
        }

        /// <summary>
        /// Handles the result of the login request to the server
        /// </summary>
        /// <param name="result">ResponseResult of the petition</param>
        /// <param name="loginOptions">Login options for this login process</param>
        static void HandleLoginResponse(ResponseResult result, LoginOptions loginOptions)
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
        static void OnLoginCancelled(LoginOptions loginOptions)
        {
            if(loginOptions.LoginFailed != null)
            {
                // Fires the provided failed callback with a report of login cancelled
                loginOptions.LoginFailed(new RequestReport(true));
            }
        }

        /// <summary>
        /// Gets a dictionary with the two required query arguments "project" and "user_email"
        /// </summary>
        /// <returns>Dictionary with the two initialized parameters</returns>
        static Dictionary<string, List<string>> GetBaseQueryArgs()
        {
            var queryVars = new Dictionary<string, List<string>>();
            queryVars.Add(_queryLogin, new List<string> { EditorPrefs.GetString(LoginWindow.LOGIN_PREF_KEY) });
            queryVars.Add(_queryProject, new List<string> { TBConfig.GetConfig().project });

            return queryVars;
        }

        #endregion

        #region PUBLIC_METHODS

        /// <summary>
        /// Sends a GetBundles request. It will trigger a login if not previously logged for this session and then sends the request
        /// </summary>
        /// <param name="arguments">Arguments needed for this type of request</param>
        public static void GetBundles(GetBundlesArgs arguments)
        {
            GenericRequest(arguments, _requestUrl, "GET", null, x => HandleActionResponse(x, arguments, GetBundles));
        }

        /// <summary>
        /// Sends a CreateBundle request. It will trigger a login if not previously logged for this session and then sends the request
        /// </summary>
        /// <param name="arguments">Arguments needed for this type of request</param>
        public static void CreateBundle(CreateBundlesArgs arguments)
        {
            GenericRequest(arguments, _requestUrl, "POST", JsonMapper.ToJson(arguments.AssetGUIDs), x => HandleActionResponse(x, arguments, CreateBundle));
        }

        /// <summary>
        /// Sends a RemoveBundle request. It will trigger a login if not previously logged for this session and then sends the request
        /// </summary>
        /// <param name="arguments">Arguments needed for this type of request</param>
        public static void RemoveBundle(RemoveBundlesArgs arguments)
        {
            GenericRequest(arguments, _removeUrl, "POST", JsonMapper.ToJson(arguments.AssetGUIDs), x => HandleActionResponse(x, arguments, RemoveBundle));
        }

        /// <summary>
        /// Sends a MakeLocalBundle request. It will trigger a login if not previously logged for this session and then sends the request
        /// </summary>
        /// <param name="arguments">Arguments needed for this type of request</param>
        public static void MakeLocalBundle(MakeLocalBundlesArgs arguments)
        {
            GenericRequest(arguments, _localBundleUrl, "POST", JsonMapper.ToJson(arguments.AssetGUIDs), x => HandleActionResponse(x, arguments, MakeLocalBundle));
        }

        /// <summary>
        /// Sends a RemoveLocalBundle request. It will trigger a login if not previously logged for this session and then sends the request
        /// </summary>
        /// <param name="arguments">Arguments needed for this type of request</param>
        public static void RemoveLocalBundle(RemoveLocalBundlesArgs arguments)
        {
            GenericRequest(arguments, _removeLocalBundleUrl, "POST", JsonMapper.ToJson(arguments.AssetGUIDs), x => HandleActionResponse(x, arguments, RemoveLocalBundle));
        }

        /// <summary>
        /// Sends a cancel order for a request.
        /// </summary>
        /// <param name="arguments">Arguments needed for this type of request</param>
        public static void CancelRequest(CancelRequestArgs arguments)
        {
            GenericRequest(arguments, _cancelUrl, "POST", JsonMapper.ToJson(arguments.requestIDs), x => HandleActionResponse(x, arguments, CancelRequest));
        }


        static void GenericRequest(RequestArgs arguments, string url, string method, string body, Action<ResponseResult> finishedCallback)
        {
            // Build up the Login Options with callbacks and options
            var options = new LoginOptions();

            options.AutoRetryLogin = arguments.AutoRetryLogin;
            options.LoginOk = report =>
            {
                var request = (HttpWebRequest)HttpWebRequest.Create(HttpAsyncRequest.AppendQueryParams(url, GetBaseQueryArgs()));
                request.Method = method;
                request.ContentType = "application/json";
                var requestData = new AsyncRequestData(request, body, finishedCallback);
                arguments.SetRequestReport(report);
                ActionRequest(arguments, requestData);
            };

            options.LoginFailed = report =>
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
        static void LoginAndExecuteAction(LoginOptions loginOptions)
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
        static void ActionRequest<T>(T arguments, AsyncRequestData requestData) where T : RequestArgs
        {
            var asyncReq = new HttpAsyncRequest(requestData);

            asyncReq.Send();
        }

        /// <summary>
        /// Generic response handler for all the requests
        /// </summary>
        /// <typeparam name="T">Type of arguments for the requets</typeparam>
        /// <param name="responseResult">Response result received by the requests</param>
        /// <param name="arguments">Arguments that were passed to the request</param>
        /// <param name="retryRequest">Method to retry the request in case of login failure</param>
        static void HandleActionResponse<T>(ResponseResult responseResult, T arguments, Action<T> retryRequest) where T : RequestArgs
        {
            if(!responseResult.IsInternal && responseResult.StatusCode == HttpStatusCode.Forbidden)
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
