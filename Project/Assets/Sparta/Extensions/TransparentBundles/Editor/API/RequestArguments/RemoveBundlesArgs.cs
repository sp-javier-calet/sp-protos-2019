using System;
using System.Collections.Generic;

namespace SocialPoint.TransparentBundles
{
    public class RemoveBundlesArgs : RequestArgs
    {
        public List<string> AssetGUIDs;

        public RemoveBundlesArgs(List<string> assetGUIDs, Action<RequestReport> SuccessCallback, Action<RequestReport> FailedCallback) : base(SuccessCallback, FailedCallback)
        {
            this.AssetGUIDs = assetGUIDs;
        }
    }
}
