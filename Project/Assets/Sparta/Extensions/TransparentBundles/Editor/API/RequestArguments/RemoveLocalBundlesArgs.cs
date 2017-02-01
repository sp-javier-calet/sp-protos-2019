using UnityEngine;
using System.Collections;
using System;

namespace SocialPoint.TransparentBundles
{
    public class RemoveLocalBundlesArgs : RequestArgs
    {
        public string AssetGUID;

        public RemoveLocalBundlesArgs(Action<RequestReport> SuccessCallback, Action<RequestReport> FailedCallback) : base(SuccessCallback, FailedCallback) { }
    }
}
