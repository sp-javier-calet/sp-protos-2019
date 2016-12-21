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
            public bool Autologin = true;
            public Action<RequestReport> LoginOk = null;
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
        public static void Login(LoginOptions loginOptions = null)
        {
            if(loginOptions == null)
            {
                loginOptions = new LoginOptions();
            }

            var loginUser = EditorPrefs.GetString(LoginWindow.LOGIN_PREF_KEY);

            if(string.IsNullOrEmpty(loginUser))
            {
                if(loginOptions.Autologin)
                {
                    LoginWindow.Open(() => Login(loginOptions), () => OnLoginCancelled(loginOptions));
                }
                else
                {
                    loginOptions.LoginFailed(new RequestReport(true, false, "No username found"));
                }
            }
            else
            {
                HttpAsyncRequest asyncReq = new HttpAsyncRequest(SERVER_URL, HttpAsyncRequest.MethodType.POST, x => HandleLoginResponse(x, loginOptions));

                asyncReq.Send();
            }
        }

        private static void HandleLoginResponse(ResponseResult result, LoginOptions loginOptions)
        {
            if(result != null && result.Success)
            {
                _isLogged = true;
                if(loginOptions.LoginOk != null)
                {
                    loginOptions.LoginOk(new RequestReport(true, false));
                }
            }
            else
            {
                if(loginOptions.Autologin)
                {
                    LoginWindow.Open(() => Login(loginOptions), () => OnLoginCancelled(loginOptions), result.Message);
                }
                else
                {
                    if(loginOptions.LoginFailed != null)
                    {
                        loginOptions.LoginFailed(new RequestReport(true, false, result));
                    }
                }
            }
        }

        private static void OnLoginCancelled(LoginOptions loginOptions)
        {
            if(loginOptions.LoginFailed != null)
            {                
                loginOptions.LoginFailed(new RequestReport(true, true, "Login cancelled by user"));
            }
        }
        #endregion

        #region PUBLIC_METHODS
        public static void CreateBundle(CreateBundlesArgs arguments, bool autoLogin = true)
        {
            var options = new LoginOptions();

            options.Autologin = autoLogin;
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

            LoginAndExecuteAction(options);
        }
        #endregion

        #region PRIVATE_METHODS
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

        private static void CreateBundleAction(RequestArgs arguments)
        {
            HttpAsyncRequest asyncReq = new HttpAsyncRequest(SERVER_URL, HttpAsyncRequest.MethodType.POST, arguments.UpdateReportAndCallback);

            asyncReq.Send();
        }

        #endregion
    }
}
