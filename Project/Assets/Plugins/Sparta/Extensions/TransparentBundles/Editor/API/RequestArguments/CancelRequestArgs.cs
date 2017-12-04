using System;
using System.Collections.Generic;

namespace SocialPoint.TransparentBundles
{
    public class CancelRequestArgs : RequestArgs
    {
        public int[] requestIDs;

        public CancelRequestArgs(int requestID, Action<RequestReport> SuccessCallback, Action<RequestReport> FailedCallback) : base(SuccessCallback, FailedCallback)
        {
            this.requestIDs = new int[] {requestID};
        }

        public CancelRequestArgs(int[] requestIDs, Action<RequestReport> SuccessCallback, Action<RequestReport> FailedCallback) : base(SuccessCallback, FailedCallback)
        {
            this.requestIDs = requestIDs;
        }
    }
}
