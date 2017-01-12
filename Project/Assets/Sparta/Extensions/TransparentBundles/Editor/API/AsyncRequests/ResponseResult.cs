using UnityEngine;
using System.Collections;
using System.Net;

namespace SocialPoint.TransparentBundles
{
    public class ResponseResult
    {
        public bool Success = false, IsInternal = true;
        public string Response = string.Empty, Message = string.Empty;
        private HttpStatusCode _statusCode;
        public HttpStatusCode StatusCode
        {
            get
            {
                if(IsInternal)
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

        public ResponseResult(bool success, string message, HttpStatusCode statusCode = 0, string response = "")
        {
            this.Success = success;
            this.Message = message;

            if(statusCode == 0 && response == "")
            {
                IsInternal = true;
            }
            else
            {
                IsInternal = false;
                this.Response = response;
                this.StatusCode = statusCode;
            }
        }
    }
}
