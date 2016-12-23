using UnityEngine;
using System.Collections;

namespace SocialPoint.TransparentBundles
{
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
}
