using System;

public abstract class RequestArgs {
    public Action<ResponseResult> OnSuccessCallback;
    public Action<ResponseResult> OnFailedCallback;

    public RequestArgs(Action<ResponseResult> SuccessCallback, Action<ResponseResult> FailedCallback)
    {
        OnSuccessCallback = SuccessCallback;
        OnFailedCallback = FailedCallback;
    }

}
