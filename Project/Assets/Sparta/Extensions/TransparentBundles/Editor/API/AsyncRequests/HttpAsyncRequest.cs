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

        public const int TIMEOUT_MILLISECONDS = 10000;

        public HttpWebRequest Request
        {
            get
            {
                return _reqState.Request;
            }
        }

        private AsyncRequestState _reqState;

        public HttpAsyncRequest(HttpWebRequest request, Action<ResponseResult> finishedCallback)
        {
            _reqState = new AsyncRequestState(request, finishedCallback);
        }

        public HttpAsyncRequest(string url, MethodType method, Action<ResponseResult> finishedCallback)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method.ToString();
            _reqState = new AsyncRequestState(request, finishedCallback);
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
            var state = (AsyncRequestState)asynchronousResult.AsyncState;
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

        private void GetResponseAsync(AsyncRequestState state)
        {
            // Start the asynchronous operation to get the response
            var asyncResult = state.Request.BeginGetResponse(new AsyncCallback(GetResponseCallback), state);

            // this line implements the timeout, if there is a timeout, the callback fires and the request becomes aborted
            ThreadPool.RegisterWaitForSingleObject(asyncResult.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback), state, TIMEOUT_MILLISECONDS, true);
        }

        private void TimeoutCallback(object stateObj, bool timeOut)
        {
            var state = (AsyncRequestState)stateObj;
            if(timeOut)
            {
                state.ConnectionFinished(new ResponseResult(false, "The request timed out"));
            }
        }

        private void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            var state = (AsyncRequestState)asynchronousResult.AsyncState;
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
