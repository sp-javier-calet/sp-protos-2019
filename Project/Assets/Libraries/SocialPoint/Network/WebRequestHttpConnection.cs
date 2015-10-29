using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Utils;
using SocialPoint.Base;
using UnityEngine;

namespace SocialPoint.Network
{
    public class WebRequestHttpConnection : BaseYieldHttpConnection
    {
        private byte[] _requestBody;
        private HttpWebRequest _request;
    
        public WebRequestHttpConnection(HttpWebRequest request, HttpResponseDelegate del, byte[] reqBody):base(del)
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
                NotifyError(HttpResponse.StatusCodeType.UnknownError, webAsync.ErrorMessage);
                yield break;
            }

            var enumeratorText = webAsync.GetResponseText(response);
            
            while(enumeratorText.MoveNext())
            { 
                yield return enumeratorText.Current; 
            }

            var resp = ConvertResponse(response, webAsync.ResponseBody.ToArray());
            OnResponse(resp);
        }
        
        void NotifyError(HttpResponse.StatusCodeType statusCode, string errorMessage)
        {
            var resp = new HttpResponse((int)statusCode);
            resp.Error = new Error((int)statusCode, errorMessage);
            OnResponse(resp);
        }

        private HttpResponse ConvertResponse(HttpWebResponse wresp, byte[] responseBody)
        {
            if(wresp == null)
            {
                return null;
            }
            
            var list = new Dictionary<string, string>();
            for(int k = 0; k < wresp.Headers.Count; k++)
            {
                list.Add(wresp.Headers.GetKey(k), wresp.Headers.Get(k));
            }

            int code = (int)wresp.StatusCode;
            var resp = new HttpResponse(code, list);
            if(resp.HasError)
            {
                resp.Error = new Error(code, wresp.StatusDescription);
            }
            resp.OriginalBody = responseBody;

            return resp;
        }

    }
}

