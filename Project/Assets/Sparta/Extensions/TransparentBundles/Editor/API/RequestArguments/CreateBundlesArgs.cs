using UnityEngine;
using System.Collections.Generic;
using System;

namespace SocialPoint.TransparentBundles
{
    public class CreateBundlesArgs : RequestArgs
    {
        public List<string> AssetGUIDs;

        public CreateBundlesArgs(List<string> assetGUIDs, Action<RequestReport> SuccessCallback, Action<RequestReport> FailedCallback) : base(SuccessCallback, FailedCallback)
        {
            this.AssetGUIDs = assetGUIDs;
        }
    }
}
