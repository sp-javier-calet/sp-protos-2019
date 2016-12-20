using System.Net;
using System.Threading;
using System.Text;
using System;
using System.IO;

namespace SocialPoint.TransparentBundles
{
    public class HttpAsyncRequest
    {
        public enum MethodType
        {
            GET,
            POST,
            DELETE
        }

        public class RequestState
        {
            public HttpWebRequest Request;
            public string RequestData;
            private ResponseResult _rResult;
            private Action<ResponseResult> _OnSuccess;
            private Action<ResponseResult> _OnFailed;

            public RequestState(HttpWebRequest request, Action<ResponseResult> successCallback, Action<ResponseResult> failedCallback)
            {
                this.Request = request;
                this.RequestData = null;
                _OnSuccess = successCallback;
                _OnFailed = failedCallback;
            }

            public RequestState(HttpWebRequest request, string requestData, Action<ResponseResult> successCallback, Action<ResponseResult> failedCallback)
            {
                this.Request = request;
                this.RequestData = requestData;
                _OnSuccess = successCallback;
                _OnFailed = failedCallback;
            }

            public void RaiseCallback()
            {
                if(_rResult.Success)
                {
                    _OnSuccess(_rResult);
                }
                else
                {
                    _OnFailed(_rResult);
                }
            }

            public void ConnectionFinished(ResponseResult result)
            {
                _rResult = result;
                MainThreadQueue.Instance.AddQueueItem(this);
            }

            public ResponseResult GetResponseResult()
            {
                return _rResult;
            }

        }

        public const int TIMEOUT_MILLISECONDS = 10000;

        public HttpWebRequest Request
        {
            get
            {
                return _reqState.Request;
            }
        }

        private RequestState _reqState;

        public HttpAsyncRequest(HttpWebRequest request, Action<ResponseResult> successCallback, Action<ResponseResult> failedCallback)
        {
            _reqState = new RequestState(request, successCallback, failedCallback);
        }

        public HttpAsyncRequest(string url, MethodType method, Action<ResponseResult> successCallback, Action<ResponseResult> failedCallback)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method.ToString();
            _reqState = new RequestState(request, successCallback, failedCallback);
        }

        public void Send()
        {
            if(_reqState.RequestData != null)
            {
                _reqState.Request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), _reqState);
            }
            else
            {
                GetResponseAsync(_reqState);
            }
        }

        private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            RequestState state = (RequestState)asynchronousResult.AsyncState;
            try
            {
                // End the operation
                Stream postStream = state.Request.EndGetRequestStream(asynchronousResult);

                if(state.RequestData != string.Empty)
                {
                    // Write to the request stream.
                    postStream.Write(Encoding.UTF8.GetBytes(state.RequestData), 0, state.RequestData.Length);
                    postStream.Close();
                }

                GetResponseAsync(state);
            }
            catch(Exception e)
            {
                state.ConnectionFinished(new ResponseResult(false, e.Message));
            }
        }

        private void GetResponseAsync(RequestState state)
        {
            // Start the asynchronous operation to get the response
            var asyncResult = state.Request.BeginGetResponse(new AsyncCallback(GetResponseCallback), state);

            // this line implements the timeout, if there is a timeout, the callback fires and the request becomes aborted
            ThreadPool.RegisterWaitForSingleObject(asyncResult.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback), state, TIMEOUT_MILLISECONDS, true);

        }

        private void TimeoutCallback(object stateObj, bool timeOut)
        {
            RequestState state = (RequestState)stateObj;
            if(timeOut)
            {
                state.ConnectionFinished(new ResponseResult(false, "The request timed out"));
            }
        }

        private void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            RequestState state = (RequestState)asynchronousResult.AsyncState;
            try
            {
                ResponseResult rResult = null;
                // End the operation
                using(HttpWebResponse response = (HttpWebResponse)state.Request.EndGetResponse(asynchronousResult))
                {
                    using(Stream streamResponse = response.GetResponseStream())
                    {
                        using(StreamReader streamRead = new StreamReader(streamResponse))
                        {
                            rResult = new ResponseResult(true, response.StatusCode + " " + response.StatusDescription, streamRead.ReadToEnd());
                        }
                    }
                }

                if(rResult == null)
                {
                    throw new Exception("Unable to read response result for url " + state.Request.RequestUri);
                }

                state.ConnectionFinished(rResult);
            }
            catch(Exception e)
            {
                state.ConnectionFinished(new ResponseResult(false, e.Message));
            }
        }
    }
}
