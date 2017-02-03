using UnityEngine;
using System.Collections;
using System;

namespace SocialPoint.TransparentBundles
{
    public class RemoveBundlesArgs : RequestArgs
    {
        public string AssetGUID;

        public RemoveBundlesArgs(Action<RequestReport> SuccessCallback, Action<RequestReport> FailedCallback) : base(SuccessCallback, FailedCallback) { }
    }
}
