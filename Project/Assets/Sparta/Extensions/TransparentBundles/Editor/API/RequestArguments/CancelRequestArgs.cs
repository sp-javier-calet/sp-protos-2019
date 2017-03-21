using System;
using System.Collections.Generic;

namespace SocialPoint.TransparentBundles
{
    public class CancelRequestArgs : RequestArgs
    {
        public int requestID;

        public CancelRequestArgs(int requestID, Action<RequestReport> SuccessCallback, Action<RequestReport> FailedCallback) : base(SuccessCallback, FailedCallback)
        {
            this.requestID = requestID;
        }
    }
}
