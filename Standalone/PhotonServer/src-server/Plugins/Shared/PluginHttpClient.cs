
using System;
using System.IO;
using Photon.Hive.Plugin;

namespace SocialPoint.Network
{
    class PluginHttpConnection : IHttpConnection
    {
        HttpResponseDelegate _callback;

        public PluginHttpConnection(HttpResponseDelegate callback)
        {
            _callback = callback;
        }

        public void Cancel()
        {
        }

        public void Release()
        {
            _callback = null;
        }

        public void OnResponse(HttpResponse resp)
        {
            if(_callback != null)
            {
                _callback(resp);
            }
        }
    }

    class PluginHttpClient : IHttpClient
    {
        public string Config
        {
            set
            {
            }
        }

        public string DefaultProxy
        {
            set
            {
            }
        }

        public event HttpRequestDelegate RequestSetup;

        public IPluginHost PluginHost;

        public PluginHttpClient(IPluginHost host = null)
        {
            PluginHost = null;
        }

        public void Dispose()
        {
        }

        public IHttpConnection Send(HttpRequest request, HttpResponseDelegate del = null)
        {
            if(RequestSetup != null)
            {
                RequestSetup(request);
            }
            if(PluginHost == null)
            {
                throw new InvalidOperationException("No plugin host specified.");
            }
            var stream = new MemoryStream();
            if(request.Body != null)
            {
                stream.Write(request.Body, 0, request.Body.Length);
            }
            var conn = new PluginHttpConnection(del);
            var photonRequest = new Photon.Hive.Plugin.HttpRequest
            {
                Url = request.Url.ToString(),
                Method = request.Method.ToString(),
                Accept = request.GetHeader(HttpRequest.AcceptEncodingHeader),
                ContentType = request.GetHeader(HttpRequest.ContentTypeHeader),
                Callback = OnRequestCallback,
                UserState = conn,
                CustomHeaders = request.Headers,
                DataStream = stream,
                Async = false
            };
            PluginHost.HttpRequest(photonRequest);
            return conn;
        }

        static void OnRequestCallback(Photon.Hive.Plugin.IHttpResponse photonResp, object userState)
        {
            var resp = new HttpResponse(photonResp.HttpCode);
            resp.OriginalBody = photonResp.ResponseData;
            var conn = userState as PluginHttpConnection;
            conn.OnResponse(resp);
        }
    }
}
