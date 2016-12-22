using UnityEngine;
using System.Collections;
using System.Net;

namespace SocialPoint.TransparentBundles
{
    public class ResponseResult
    {
        public bool Success = false, IsInternal = false;
        public string Response = string.Empty, Message = string.Empty;
        private HttpStatusCode _statusCode;
        public HttpStatusCode StatusCode
        {
            get
            {
                if(!IsInternal)
                {
                    throw new System.Exception("Accessing the Status Code of a request with internal error " + Message);
                }

                return _statusCode;
            }
            set
            {
                _statusCode = value;
            }
        }

        public ResponseResult()
        {
        }

        public ResponseResult(bool success, string message, string response = "")
        {
            this.Success = success;
            this.Message = message;
            this.Response = response;
        }
    }
}
