using UnityEngine;
using System.Collections;

public class ResponseResult
{
    public bool success;
    public string response;
    public string message;

    public ResponseResult() { }

    public ResponseResult(bool success, string message)
    {
        this.success = success;
        this.message = message;
    }

}
