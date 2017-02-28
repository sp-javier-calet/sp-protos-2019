using System;

namespace SocialPoint.TransparentBundles
{
    public class GetBundlesArgs : RequestArgs
    {
        public GetBundlesArgs(Action<RequestReport> SuccessCallback, Action<RequestReport> FailedCallback) : base(SuccessCallback, FailedCallback)
        {
        }
    }
}
