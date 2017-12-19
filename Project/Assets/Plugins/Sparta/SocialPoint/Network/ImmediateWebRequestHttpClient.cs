using System;
using System.IO;
using System.Net;

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

        public ImmediateWebRequestHttpClient()
        {
            RequestSetup += OnRequestSetup;
        }

        static void OnRequestSetup(HttpRequest req)
        {
            var editorProxy = EditorProxy.GetProxy();
            if(string.IsNullOrEmpty(req.Proxy) && !string.IsNullOrEmpty(editorProxy))
            {
                req.Proxy = editorProxy;
            }
        }

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
                webRequest.ContentLength = request.Body.Length;

                var reqStream = webRequest.GetRequestStream();
                reqStream.Write(request.Body, 0, request.Body.Length);
                reqStream.Close();
                reqStream.Dispose();
            }
            HttpWebResponse webResponse;

            try
            {
                webResponse = webRequest.GetResponse() as HttpWebResponse;
            }
            catch(WebException ex)
            {
                webResponse = ex.Response as HttpWebResponse;
                if(webResponse == null)
                {
                    throw;
                }
            }
            if(webResponse == null)
            {
                throw new InvalidOperationException("Could not get a response object.");
            }
            var respStream = webResponse.GetResponseStream();
            var buffer = new byte[BufferLength];
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

        public string Config
        {
            set
            {
            }
        }

        public void Dispose()
        {
            RequestSetup -= OnRequestSetup;
        }
    }
}
