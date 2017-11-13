using System;
using System.Collections;
using System.Net;
using System.IO;
using System.Threading;
using SocialPoint.Base;

namespace SocialPoint.Network
{
    /// <summary>
    /// Simplify getting web requests asynchronously
    /// </summary>
    public sealed class WebAsync
    {
        float _timeout;
        // seconds
        bool _readingResponse = false;

        public bool IsResponseTimeOut = false;
        public bool IsResponseCompleted = false;
        public WebRequest WebRequest = null;
        public WebResponse WebResponse = null;
        public byte[] BufferRead = new byte[1024];
        public string ErrorMessage = null;
        public MemoryStream ResponseBody = new MemoryStream();
        public bool IsUrlcheckingCompleted = false;
        public bool IsUrlMissing = false;

        public WebAsync(float timeout = 10)
        {
            _timeout = timeout;
        }

        public IEnumerator SetPostData(HttpWebRequest webRequest, byte[] postData)
        {
            IAsyncResult asyncResult = null;

            try
            {
                asyncResult = (IAsyncResult)webRequest.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), webRequest);
            }
            catch(Exception exception)
            {
                Log.e("[WebAsync] Error message while getting stream from request '" + webRequest.RequestUri.ToString() + "': " + exception.Message);
                ErrorMessage = exception.Message;
                yield break;
            }

            // Put the request into the state object so it can be passed around
            WebRequest = webRequest;

            if(_timeout > 0)
            {
                // WebRequest timeout won't work in async calls, so we need this instead
                ThreadPool.RegisterWaitForSingleObject(
                    asyncResult.AsyncWaitHandle,
                    new WaitOrTimerCallback(ScanTimeoutCallback),
                    this,
                    (int)(_timeout * 1000), // obviously because this is in miliseconds
                    true
                );
            }

            // Wait until the the call is completed
            while(!asyncResult.IsCompleted)
            {
                yield return null;
            }

            try
            {
                var stream = webRequest.EndGetRequestStream(asyncResult);
                stream.Write(postData, 0, postData.Length);
                stream.Close();
            }
            catch(WebException webException)
            {
                Log.e("[WebAsync] Error message while getting stream from request '" + webRequest.RequestUri.ToString() + "': " + webException.Message);
                ErrorMessage = webException.Message;
            }
        }

        public IEnumerator GetResponseText(HttpWebResponse webRequest)
        {
            _readingResponse = true;

            Stream stream = null;

            while(stream == null)
            {
                stream = webRequest.GetResponseStream();
                yield return null;
            }

            stream.BeginRead(BufferRead, 0, 1024, new AsyncCallback(ReadCallBack), webRequest);

            while(_readingResponse)
            {
                yield return null;
            }
        }

        static void GetRequestStreamCallback(IAsyncResult asyncResult)
        {
        }

        void ReadCallBack(IAsyncResult asyncResult)
        {
            var webRequest = (HttpWebResponse)asyncResult.AsyncState;
            var responseStream = webRequest.GetResponseStream();

            int read = responseStream.EndRead(asyncResult);

            try
            {
                if(read > 0)
                {
                    ResponseBody.Write(BufferRead, 0, read);
                    responseStream.BeginRead(BufferRead, 0, BufferRead.Length, new AsyncCallback(ReadCallBack), webRequest);
                }
                else
                {
                    _readingResponse = false;
                    responseStream.Close();
                }
            }
            catch(Exception ex)
            {
                Log.e("[WebAsync] Error message while readingResponse '" + webRequest.ResponseUri.ToString() + "': " + ex.Message);
                _readingResponse = false;
            }
        }

        /// <summary>
        /// Equivalent of webRequest.GetResponse, but using our own RequestState.
        /// This can or should be used along with web async instance's isResponseCompleted parameter
        /// inside a IEnumerator method capable of yield return for it, although it's mostly for clarity.
        /// Here's an usage example:
        ///
        /// WebAsync webAsync = new WebAsync(); StartCoroutine( webAsync.GetReseponse(webRequest) );
        /// while (! webAsync.isResponseCompleted) yield return null;
        /// RequestState result = webAsync.requestState;
        ///
        /// </summary>
        /// <param name='webRequest'>
        /// A System.Net.WebRequest instanced var.
        /// </param>
        public IEnumerator GetResponse(HttpWebRequest webRequest)
        {
            IsResponseCompleted = false;

            // Put the request into the state object so it can be passed around
            WebRequest = webRequest;

            // Do the actual async call here
            IAsyncResult asyncResult = (IAsyncResult)webRequest.BeginGetResponse(new AsyncCallback(GetResponseCallback), this);

            // WebRequest timeout won't work in async calls, so we need this instead
            ThreadPool.RegisterWaitForSingleObject(
                asyncResult.AsyncWaitHandle,
                new WaitOrTimerCallback(ScanTimeoutCallback),
                this,
                (int)(_timeout * 1000), // obviously because this is in milliseconds
                true
            );

            // Wait until the the call is completed
            while(!asyncResult.IsCompleted || WebResponse == null)
            {
                yield return null;
            }

#if SPHTTP_PRINT_ERRORS
            // Help debugging possibly unpredictable results
            if(requestState != null)
            {
                if(requestState.errorMessage != null)
                {
                    // this is not an ERROR because there are at least 2 error messages that are expected: 404 and NameResolutionFailure - as can be seen on CheckForMissingURL
                    var uri = "";
                    if(webRequest != null && webRequest.RequestUri != null)
                    {
                        uri = webRequest.RequestUri.ToString();
                    }
                    Log.d("[WebAsync] Error message while getting response from request '" + uri + "': " + requestState.errorMessage);
                }
            }
#endif
            IsResponseCompleted = true;
        }

        void GetResponseCallback(IAsyncResult asyncResult)
        {
            try
            {
                WebResponse = WebRequest.EndGetResponse(asyncResult);
            }
            catch(WebException ex)
            {
                WebResponse = ex.Response;
                ErrorMessage = "From callback, " + ex.Message;
            }
        }

        void ScanTimeoutCallback(object obj, bool timedOut)
        {
            if(timedOut)
            {
                var self = (WebAsync)obj;
                if(self != null)
                {
                    IsResponseTimeOut = true;
                    self.WebRequest.Abort();
                }
            }
            else
            {
                var handle = obj as RegisteredWaitHandle;
                if(handle != null)
                {
                    handle.Unregister(null);
                }
            }
        }
    }
}
