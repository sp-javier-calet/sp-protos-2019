using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class ImmediateWebRequestHttpConnection : IHttpConnection
    {
        public void Cancel()
        {
        }

        public void Release()
        {
        }
    }


    public class ImmediateWebRequestHttpClient : IHttpClient
    {
        const int BufferLength = 16 * 1024;

        public event HttpRequestDelegate RequestSetup;

        public IHttpConnection Send(HttpRequest request, HttpResponseDelegate del = null)
        {
            if(RequestSetup != null)
            {
                RequestSetup(request);
            }
            request.Timeout = 0.0f;
            request.ActivityTimeout = 0.0f;
            var webRequest = WebRequestUtils.ConvertRequest(request);
            if(request.Body != null)
            {
                var reqStream = webRequest.GetRequestStream();
                reqStream.Write(request.Body, 0, request.Body.Length);
                reqStream.Close();
                reqStream.Dispose();
            }
            var webResponse = webRequest.GetResponse() as HttpWebResponse;
            if(webResponse == null)
            {
                throw new InvalidOperationException("Could not get a response object.");
            }
            var respStream = webResponse.GetResponseStream();
            byte[] buffer = new byte[BufferLength];
            var ms = new MemoryStream();
            int read;
            while((read = respStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, read);
            }
            respStream.Dispose();
            var resp = WebRequestUtils.ConvertResponse(webResponse, ms.ToArray());
            ms.Dispose();
            if(del != null)
            {
                del(resp);
            }
            return new ImmediateWebRequestHttpConnection();
        }

        public string DefaultProxy
        {
            set
            {
            }
        }

        public string Config
        {
            set
            {
            }
        }

        public void Dispose()
        {
        }



    }
}
