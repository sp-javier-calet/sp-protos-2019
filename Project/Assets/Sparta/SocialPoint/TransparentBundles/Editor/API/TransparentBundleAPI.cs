using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEditor;

public class TransparentBundleAPI
{
    private static bool isLogged = false;
    private static Action<RequestArgs> delayedCallback = null;
    private static RequestArgs delayedArguments = null;

    public const string SERVER_URL = "http://httpbin.org/post";
        
    [MenuItem("SocialPoint/Test Call")]
    public static void test()
    {
        CreateBundle(new CreateBundlesArgs(x=>Debug.Log(x.response), x=>Debug.Log(x.message)));
    }    

    #region LOGIN
    public static void Login()
    {
        var loginUser = EditorPrefs.GetString(LoginWindow.LOGIN_PREF_KEY);

        if(string.IsNullOrEmpty(loginUser))
        {
            LoginWindow.Open(Login, () => delayedCallback = null);
        } else
        {
            HttpAsyncRequest asyncReq = new HttpAsyncRequest(SERVER_URL, HttpAsyncRequest.MethodType.POST, OnLoginSuccess, OnLoginFailed);

            asyncReq.Send();
        }
    }
    
    private static void OnLoginSuccess(ResponseResult result)
    {
        Debug.Log("OK LOGIN");
        isLogged = true;
        if(delayedCallback != null)
        {
            delayedCallback(delayedArguments);
            delayedCallback = null;
        }
    }

    private static void OnLoginFailed(ResponseResult result)
    {
        Debug.Log("FAIL LOGIN");
        LoginWindow.Open(Login, () => delayedCallback = null, result.message);
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
        if(!isLogged)
        {
            delayedArguments = arguments;
            delayedCallback = action;
            Login();
        } else
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
