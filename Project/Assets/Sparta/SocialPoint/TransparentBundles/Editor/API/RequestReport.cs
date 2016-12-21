using UnityEngine;
using System.Collections;

public class RequestReport
{
    public bool TriedToLogin = false;
    public bool LoginCancelled = false;
    public string LoginMessage = "";
    public ResponseResult ResponseRes;
    
    public RequestReport(bool triedToLogin, bool loginCancelled, string loginMessage = "")
    {
        TriedToLogin = triedToLogin;
        LoginCancelled = loginCancelled;
        LoginMessage = loginMessage;
    }

    public RequestReport(bool triedToLogin, bool loginCancelled, ResponseResult loginResult)
    {
        TriedToLogin = triedToLogin;
        LoginCancelled = loginCancelled;
        ResponseRes = loginResult;
    }
}
