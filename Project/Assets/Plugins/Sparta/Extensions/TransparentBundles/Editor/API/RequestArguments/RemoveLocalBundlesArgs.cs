using System;
using System.Collections.Generic;

namespace SocialPoint.TransparentBundles
{
    public class RemoveLocalBundlesArgs : RequestArgs
    {
        public List<string> AssetGUIDs;

        public RemoveLocalBundlesArgs(List<string> assetGUIDs, Action<RequestReport> SuccessCallback, Action<RequestReport> FailedCallback) : base(SuccessCallback, FailedCallback)
        {
            AssetGUIDs = assetGUIDs;
        }
    }
}
