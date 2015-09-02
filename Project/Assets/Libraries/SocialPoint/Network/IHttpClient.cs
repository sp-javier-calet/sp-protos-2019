using System;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public delegate void HttpRequestDelegate(HttpRequest r);
    public delegate void HttpResponseDelegate(HttpResponse r);

    public interface IHttpConnection
    {
        void Cancel();
    }

    public interface IHttpClient : IDisposable
    {
        IHttpConnection Send(HttpRequest request, HttpResponseDelegate del = null);

        [Obsolete("Please use the RequestSetup event to set default request values")]
        string DefaultProxy { set; }

        event HttpRequestDelegate RequestSetup;

        void OnApplicationPause(bool pause);

    }
}
