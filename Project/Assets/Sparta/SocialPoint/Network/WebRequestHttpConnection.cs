using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Network
{
    public sealed class WebRequestHttpConnection : BaseYieldHttpConnection
    {
        byte[] _requestBody;
        readonly HttpWebRequest _request;

        public WebRequestHttpConnection(HttpWebRequest request, HttpResponseDelegate del, byte[] reqBody) : base(del)
        {
            _request = request;
            _requestBody = reqBody;
        }

        public override void Cancel()
        {
            _request.Abort();
        }

        public override IEnumerator Update()
        {
            var timeout = _request.Timeout;
            if(timeout == 0)
            {
                timeout = _request.ReadWriteTimeout;
            }
            var webAsync = new WebAsync(timeout / 1000.0f);

            if(_requestBody != null && _requestBody.Length > 0)
            {
                IEnumerator enumeratorPostData = webAsync.SetPostData(_request, _requestBody);
                while(enumeratorPostData.MoveNext())
                { 
                    yield return enumeratorPostData.Current; 
                }

                if(webAsync.ErrorMessage != null)
                {
                    NotifyError(HttpResponse.StatusCodeType.ConnectionFailedError, webAsync.ErrorMessage);
                    yield break;
                }
            }

            var enumeratorResponse = webAsync.GetResponse(_request);
            
            while(enumeratorResponse.MoveNext())
            { 
                yield return enumeratorResponse.Current; 
            }

            if(webAsync.IsResponseTimeOut)
            {
                NotifyError(HttpResponse.StatusCodeType.TimeOutError, webAsync.ErrorMessage);
                yield break;
            }

            var response = (HttpWebResponse)webAsync.WebResponse;
            if(response == null)
            {
                NotifyError(HttpResponse.MinClientUnknownErrorStatusCode, webAsync.ErrorMessage);
                yield break;
            }

            var enumeratorText = webAsync.GetResponseText(response);
            
            while(enumeratorText.MoveNext())
            { 
                yield return enumeratorText.Current; 
            }

            var resp = WebRequestUtils.ConvertResponse(response, webAsync.ResponseBody.ToArray());
            OnResponse(resp);
        }

        void NotifyError(HttpResponse.StatusCodeType statusCode, string errorMessage)
        {
            NotifyError((int)statusCode, errorMessage);
        }

        void NotifyError(int statusCode, string errorMessage)
        {
            var resp = new HttpResponse(statusCode);
            resp.Error = new Error(statusCode, errorMessage);
            OnResponse(resp);
        }
    }
}

