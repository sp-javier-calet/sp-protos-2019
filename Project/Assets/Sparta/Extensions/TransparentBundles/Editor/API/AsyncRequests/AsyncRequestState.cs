using System;
using System.Net;

namespace SocialPoint.TransparentBundles
{
    public class AsyncRequestState
    {
        public HttpWebRequest Request;
        public string RequestData;
        private ResponseResult _rResult;
        private Action<ResponseResult> _finishedCallback;

        public AsyncRequestState(HttpWebRequest request, Action<ResponseResult> finishedCallback)
        {
            this.Request = request;
            this.RequestData = null;
            _finishedCallback = finishedCallback;
        }

        public AsyncRequestState(HttpWebRequest request, string requestData, Action<ResponseResult> finishedCallback)
        {
            this.Request = request;
            this.RequestData = requestData;
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
        /// This raises the callback setted when the connection process is finished. 
        /// This should only be called from a MainThreadQueue attached class in order to allow Unity API calls. (WebRequestQueueHandler)
        /// </summary>
        public void RaiseCallback()
        {
            _finishedCallback(_rResult);
        }
    }
}
