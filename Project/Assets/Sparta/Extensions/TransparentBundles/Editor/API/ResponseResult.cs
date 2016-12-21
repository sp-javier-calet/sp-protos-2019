using UnityEngine;
using System.Collections;

public class ResponseResult
{
    public bool Success;
    public string Response;
    public string Message;

    public ResponseResult() { }

    public ResponseResult(bool success, string message, string response = "")
    {
        this.Success = success;
        this.Message = message;
        this.Response = response;
    }

}
