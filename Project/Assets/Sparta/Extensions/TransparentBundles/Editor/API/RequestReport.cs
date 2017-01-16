using UnityEngine;
using System.Collections;

namespace SocialPoint.TransparentBundles
{
    public class RequestReport
    {
        public bool RequestCancelled = false;
        public ResponseResult ResponseRes;

        public RequestReport(bool requestCancelled = false)
        {
            this.RequestCancelled = requestCancelled;
        }

        public RequestReport(ResponseResult loginResult, bool requestCancelled = false)
        {
            RequestCancelled = requestCancelled;
            ResponseRes = loginResult;
        }
    }
}
