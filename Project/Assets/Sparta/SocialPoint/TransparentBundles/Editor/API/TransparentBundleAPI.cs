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
    private static System.Action<IRequestArgs> delayedCallback;
    private static IRequestArgs delayedArguments;

    public const string SERVER_URL = "http://httpbin.org/post";
    
    public static void Login()
    {
        var loginUser = EditorPrefs.GetString(LoginWindow.LOGIN_PREF_KEY);

        Debug.Log(loginUser);

        if(string.IsNullOrEmpty(loginUser))
        {
            LoginWindow.Open(Login);
        } else
        {
            HttpAsyncRequest asyncReq = new HttpAsyncRequest(SERVER_URL, HttpAsyncRequest.MethodType.POST, x => { Debug.Log("OK LOGIN"); isLogged = true; delayedCallback(delayedArguments); }, x => Debug.Log(x.message));
            asyncReq.Send();
        }
    }
    
    [MenuItem("SocialPoint/Test Call")]
    public static void test()
    {
        CreateBundle();
    }

    public static void CreateBundle(IRequestArgs arguments = null)
    {
        if(!isLogged)
        {
            delayedArguments = arguments;
            delayedCallback = CreateBundle;
            Login();
        } else
        {
            HttpAsyncRequest asyncReq = new HttpAsyncRequest(SERVER_URL, HttpAsyncRequest.MethodType.POST, x => Debug.Log(x.response), x => Debug.Log(x.message));

            asyncReq.Send();
        }
    }
}
