using System;
using System.Net;

namespace SocialPoint.TransparentBundles
{
    public class AsyncRequestData
    {
        public HttpWebRequest Request;
        public string RequestBody;
        ResponseResult _rResult;
        readonly Action<ResponseResult> _finishedCallback;

        public AsyncRequestData(HttpWebRequest request, Action<ResponseResult> finishedCallback)
        {
            Request = request;
            RequestBody = null;
            _finishedCallback = finishedCallback;
        }

        public AsyncRequestData(HttpWebRequest request, string requestBody, Action<ResponseResult> finishedCallback)
        {
            Request = request;
            RequestBody = requestBody;
            _finishedCallback = finishedCallback;
        }

        /// <summary>
        /// This stores the result of a connection and queues the callback call for the main thread to execute
        /// </summary>
        /// <param name="result">result of the connection</param>
        public void ConnectionFinished(ResponseResult result)
        {
            _rResult = result;
            MainThreadQueue.Instance.AddQueueItem(this);
        }

        /// <summary>
        /// This raises the setted callback when the connection process is finished. 
        /// This should only be called from a MainThreadQueue attached class in order to allow Unity API calls. (WebRequestQueueHandler)
        /// </summary>
        public void RaiseCallback()
        {
            _finishedCallback(_rResult);
        }
    }
}
