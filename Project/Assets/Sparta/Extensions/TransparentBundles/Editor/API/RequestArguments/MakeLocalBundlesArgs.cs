using UnityEngine;
using System.Collections;
using System;

namespace SocialPoint.TransparentBundles
{
    public class MakeLocalBundlesArgs : RequestArgs
    {
        public string AssetGUID;

        public MakeLocalBundlesArgs(Action<RequestReport> SuccessCallback, Action<RequestReport> FailedCallback) : base(SuccessCallback, FailedCallback) { }
    }
}
