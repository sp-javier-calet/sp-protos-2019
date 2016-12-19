using UnityEngine;
using System.Collections;
using System;

public class CreateBundlesArgs : RequestArgs {
    public string assetGUID;

    public CreateBundlesArgs(Action<ResponseResult> SuccessCallback, Action<ResponseResult> FailedCallback) : base(SuccessCallback, FailedCallback) { }
}
