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
            var webRequest = WebRequestUtils.ConvertRequest(request);
            var dataStream = webRequest.GetRequestStream();
            if(request.Body != null)
            {
                dataStream.Write(request.Body, 0, request.Body.Length);
            }
            dataStream.Close();
            var webResponse = webRequest.GetResponse() as HttpWebResponse;
            if(webResponse == null)
            {
                throw new InvalidOperationException("Could not get a response object.");
            }
            dataStream = webResponse.GetResponseStream();
            byte[] buffer = new byte[BufferLength];
            var ms = new MemoryStream();
            int read;
            while((read = dataStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, read);
            }
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
