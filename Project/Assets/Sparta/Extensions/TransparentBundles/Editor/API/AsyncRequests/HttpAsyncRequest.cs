using System.Net;
using System.Threading;
using System.Text;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;

namespace SocialPoint.TransparentBundles
{
    public class HttpAsyncRequest
    {
        public enum MethodType
        {
            GET,
            POST,
            DELETE,
            HEAD,
            OPTIONS,
            PUT,
            TRACE
        }

        public const int TIMEOUT_MILLISECONDS = 10000;

        public HttpWebRequest Request
        {
            get
            {
                if(locked)
                {
                    throw new Exception("You are trying to get a Request that is in process, this is not allowed");
                }

                return _reqState.Request;
            }
        }

        private AsyncRequestData _reqState;
        private bool locked = false;

        public HttpAsyncRequest(AsyncRequestData requestData)
        {
            _reqState = requestData;
        }

        public HttpAsyncRequest(HttpWebRequest request, Action<ResponseResult> finishedCallback) : this(new AsyncRequestData(request, finishedCallback)) { }

        public HttpAsyncRequest(HttpWebRequest request, string requestBody, Action<ResponseResult> finishedCallback) : this(new AsyncRequestData(request, requestBody, finishedCallback)) { }

        public HttpAsyncRequest(string url, MethodType method, Action<ResponseResult> finishedCallback)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method.ToString();
            _reqState = new AsyncRequestData(request, finishedCallback);
        }
        /// <summary>
        /// Given a base URL and a dictionary of params, appends params to the url string into the query
        /// </summary>
        /// <param name="url">Base url</param>
        /// <param name="queryParams">query params to append</param>
        /// <returns>Full url with all the query params</returns>
        public static string GetURLWithQuery(string url, Dictionary<string, string> queryParams)
        {
            if(queryParams.Count > 0)
            {
                url += "?";

                foreach(var pair in queryParams)
                {
                    url += pair.Key + "=" + pair.Value + "&";
                }

                url = url.TrimEnd('&');
            }

            return url;
        }

        public bool CertificateValidation(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        /// <summary>
        /// Start the process of sending the request and receiving the response asynchronously. No modifications should be made to this request after this call.
        /// </summary>
        public void Send()
        {
            locked = true;

            ServicePointManager.ServerCertificateValidationCallback += CertificateValidation;

            if(!string.IsNullOrEmpty(_reqState.RequestBody))
            {
                _reqState.Request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), _reqState);
            }
            else
            {
                GetResponseAsync(_reqState);
            }
        }

        /// <summary>
        /// Ends the RequestStream asynchronous process and starts the GetResponse
        /// </summary>
        private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            var state = (AsyncRequestData)asynchronousResult.AsyncState;
            try
            {
                // End the operation
                Stream postStream = state.Request.EndGetRequestStream(asynchronousResult);
                
                // Write to the request stream.
                postStream.Write(Encoding.UTF8.GetBytes(state.RequestBody), 0, state.RequestBody.Length);
                postStream.Close();

                GetResponseAsync(state);
            }
            catch(Exception e)
            {
                EndConnection(state, new ResponseResult(false, e.Message));
            }
        }

        /// <summary>
        /// Starts the GetResponse asynchronously
        /// </summary>
        /// <param name="state">AsyncRequestState context</param>
        private void GetResponseAsync(AsyncRequestData state)
        {
            // Start the asynchronous operation to get the response
            var asyncResult = state.Request.BeginGetResponse(new AsyncCallback(GetResponseCallback), state);

            // this line implements the timeout, if there is a timeout, the callback fires and the request becomes aborted
            ThreadPool.RegisterWaitForSingleObject(asyncResult.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback), state, TIMEOUT_MILLISECONDS, true);
        }

        /// <summary>
        /// Callback in case the get response takes too long
        /// </summary>
        /// <param name="stateObj">AsyncRequestState context</param>
        /// <param name="timeOut">wether or not the request timed out</param>
        private void TimeoutCallback(object stateObj, bool timeOut)
        {
            var state = (AsyncRequestData)stateObj;
            if(timeOut)
            {
                EndConnection(state, new ResponseResult(false, "The request timed out", HttpStatusCode.RequestTimeout));
            }
        }

        /// <summary>
        /// Ends the GetResponse and handles the result
        /// </summary>
        private void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            var state = (AsyncRequestData)asynchronousResult.AsyncState;
            try
            {
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
                                rResult = new ResponseResult(true, streamRead.ReadToEnd(), response.StatusCode);
                                rResult.StatusCode = response.StatusCode;
                            }
                        }
                    }

                    if(rResult == null)
                    {
                        throw new Exception("Unable to read response result for url " + state.Request.RequestUri);
                    }

                    EndConnection(state, rResult);
                }
                catch(WebException e)
                {
                    var resp = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                    string jsonMsg = string.Empty;
                    try
                    {
                        jsonMsg = LitJson.JsonMapper.ToObject(resp)[0].ToString();
                    }
                    catch(Exception ex)
                    {
                        UnityEngine.Debug.LogError(ex);
                        jsonMsg = resp;
                    }

                    EndConnection(state, new ResponseResult(false, e.Message + " - " + jsonMsg, ((HttpWebResponse)e.Response).StatusCode));
                }
            }
            catch(Exception e)
            {
                EndConnection(state, new ResponseResult(false, e.Message));
            }

        }

        /// <summary>
        /// Finishes the connection process for this request
        /// </summary>
        /// <param name="state">AsyncRequestState context object</param>
        /// <param name="result">Result of the connection</param>
        private void EndConnection(AsyncRequestData state, ResponseResult result)
        {
            locked = false;
            ServicePointManager.ServerCertificateValidationCallback -= CertificateValidation;
            state.ConnectionFinished(result);
        }
    }
}
