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
        private Action<ResponseResult> _onFailed;

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

        public void ConnectionFinished(ResponseResult result)
        {
            _rResult = result;
            MainThreadQueue.Instance.AddQueueItem(this);
        }

        public void RaiseCallback()
        {
            _finishedCallback(_rResult);
        }
    }
}
