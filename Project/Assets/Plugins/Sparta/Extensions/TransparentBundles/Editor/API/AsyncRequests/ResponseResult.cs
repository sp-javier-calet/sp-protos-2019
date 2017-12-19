using System.Net;
using LitJson;

namespace SocialPoint.TransparentBundles
{
    public class ResponseResult
    {
        public bool Success, IsInternal = true;
        public string Response = string.Empty;
        HttpStatusCode _statusCode;

        public HttpStatusCode StatusCode
        {
            get
            {
                if(IsInternal)
                {
                    throw new System.Exception("Accessing the Status Code of a request with internal error " + Response);
                }

                return _statusCode;
            }
            private set
            {
                _statusCode = value;
            }
        }

        public ResponseResult()
        {
        }

        public ResponseResult(bool success, string response, HttpStatusCode statusCode = 0)
        {
            Success = success;
            Response = response;

            if(statusCode == 0)
            {
                IsInternal = true;
            }
            else
            {
                IsInternal = false;
                Response = response;
                StatusCode = statusCode;
            }
        }

        public T ParseResponseAsJson<T>()
        {
            return JsonMapper.ToObject<T>(Response);
        }

        public JsonData ParseResponseAsJsonGeneric()
        {
            return JsonMapper.ToObject(Response);
        }
    }
}
