using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEditor;

namespace SocialPointEditor.TransparentBundles
{
    public class TransparentBundleAPI
    {
        private static bool _isLogged = false;
        private static Action<RequestArgs> _delayedCallback = null;
        private static RequestArgs _delayedArguments = null;

        public const string SERVER_URL = "http://httpbin.org/post";

        [MenuItem("SocialPoint/Test Call")]
        public static void test()
        {
            CreateBundle(new CreateBundlesArgs(x => Debug.Log(x.Response), x => Debug.Log(x.Message)));
        }

        #region LOGIN
        public static void Login()
        {
            var loginUser = EditorPrefs.GetString(LoginWindow.LOGIN_PREF_KEY);

            if(string.IsNullOrEmpty(loginUser))
            {
                LoginWindow.Open(Login, () => _delayedCallback = null);
            }
            else
            {
                HttpAsyncRequest asyncReq = new HttpAsyncRequest(SERVER_URL, HttpAsyncRequest.MethodType.POST, OnLoginSuccess, OnLoginFailed);

                asyncReq.Send();
            }
        }

        private static void OnLoginSuccess(ResponseResult result)
        {
            _isLogged = true;
            if(_delayedCallback != null)
            {
                _delayedCallback(_delayedArguments);
                _delayedCallback = null;
            }
        }

        private static void OnLoginFailed(ResponseResult result)
        {
            LoginWindow.Open(Login, () => _delayedCallback = null, result.Message);
        }
        #endregion

        #region PUBLIC_METHODS
        public static void CreateBundle(CreateBundlesArgs arguments)
        {
            LoginAndExecuteAction(CreateBundleAction, arguments);
        }

        #endregion


        #region PRIVATE_METHODS
        private static void LoginAndExecuteAction(Action<RequestArgs> action, RequestArgs arguments)
        {
            if(!_isLogged)
            {
                _delayedArguments = arguments;
                _delayedCallback = action;
                Login();
            }
            else
            {
                action(arguments);
            }
        }

        private static void CreateBundleAction(RequestArgs arguments)
        {
            HttpAsyncRequest asyncReq = new HttpAsyncRequest(SERVER_URL, HttpAsyncRequest.MethodType.POST, arguments.OnSuccessCallback, arguments.OnFailedCallback);

            asyncReq.Send();
        }

        #endregion
    }
}
