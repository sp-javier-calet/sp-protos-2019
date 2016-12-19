using System.Net;
using System.Threading;
using System.Text;
using System;
using System.IO;

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
        public HttpWebRequest request;
        public string requestData;
        private ResponseResult rResult;
        private Action<ResponseResult> OnSuccess;
        private Action<ResponseResult> OnFailed;

        public RequestState(HttpWebRequest request, Action<ResponseResult> successCallback, Action<ResponseResult> failedCallback)
        {
            this.request = request;
            this.requestData = null;
            OnSuccess = successCallback;
            OnFailed = failedCallback;
        }

        public RequestState(HttpWebRequest request, string requestData, Action<ResponseResult> successCallback, Action<ResponseResult> failedCallback)
        {
            this.request = request;
            this.requestData = requestData;
            OnSuccess = successCallback;
            OnFailed = failedCallback;
        }

        public void RaiseCallback()
        {
            if(rResult.success)
            {
                OnSuccess(rResult);
            } else
            {
                OnFailed(rResult);
            }
        }

        public void ConnectionFinished(ResponseResult result)
        {
            rResult = result;
            MainThreadQueue.AddQueueItem(this);
        }

        public ResponseResult GetResponseResult()
        {
            return rResult;
        }

    }

    public const int TIMEOUT_MILLISECONDS = 10000;

    public HttpWebRequest Request
    {
        get
        {
            return reqState.request;
        }
    }

    private RequestState reqState;

    public HttpAsyncRequest(HttpWebRequest request, Action<ResponseResult> successCallback, Action<ResponseResult> failedCallback)
    {
        reqState = new RequestState(request, successCallback, failedCallback);
    }

    public HttpAsyncRequest(string url, MethodType method, Action<ResponseResult> successCallback, Action<ResponseResult> failedCallback)
    {
        var request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = method.ToString();
        reqState = new RequestState(request, successCallback, failedCallback);
    }    

    public void Send()
    {
        if(reqState.requestData != null)
        {            
            reqState.request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), reqState);
        }else
        {
            GetResponseAsync(reqState);
        }
    }

    private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
    {
        RequestState state = (RequestState)asynchronousResult.AsyncState;
        try
        {
            // End the operation
            Stream postStream = state.request.EndGetRequestStream(asynchronousResult);

            if(state.requestData != string.Empty)
            {
                // Write to the request stream.
                postStream.Write(Encoding.UTF8.GetBytes(state.requestData), 0, state.requestData.Length);
                postStream.Close();
            }

            GetResponseAsync(state);
        } catch(Exception e)
        {
            state.ConnectionFinished(new ResponseResult(false, e.Message));
        }
    }

    private void GetResponseAsync(RequestState state)
    {
        // Start the asynchronous operation to get the response
        var asyncResult = state.request.BeginGetResponse(new AsyncCallback(GetResponseCallback), state);

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
            using(HttpWebResponse response = (HttpWebResponse)state.request.EndGetResponse(asynchronousResult))
            {
                using(Stream streamResponse = response.GetResponseStream())
                {
                    using(StreamReader streamRead = new StreamReader(streamResponse))
                    {
                        rResult = new ResponseResult(true, "OK");
                        rResult.response = streamRead.ReadToEnd();
                    }
                }
            }

            if(rResult == null)
            {
                throw new Exception("Unable to read response result for url " + state.request.RequestUri);
            }

            state.ConnectionFinished(rResult);
        } catch(Exception e)
        {
            state.ConnectionFinished(new ResponseResult(false, e.Message));
        }
    }
}
