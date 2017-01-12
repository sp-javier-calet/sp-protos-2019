using UnityEngine;
using System.Collections;
using System;
using System.Net;

namespace SocialPoint.TransparentBundles
{
    public class CreateBundlesArgs : RequestArgs
    {
        public string AssetGUID;

        public CreateBundlesArgs(Action<RequestReport> SuccessCallback, Action<RequestReport> FailedCallback) : base(SuccessCallback, FailedCallback) { }
    }
}
