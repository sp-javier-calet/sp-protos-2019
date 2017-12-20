using System;

namespace SocialPoint.Network
{
    public delegate void HttpRequestDelegate(HttpRequest r);
    public delegate void HttpResponseDelegate(HttpResponse r);

    public interface IHttpConnection
    {
        /// <summary>
        /// Should cancel the connection and call the callback with an error response
        /// </summary>
        void Cancel();

        /// <summary>
        /// Will maintain the connection but not call the callback
        /// </summary>
        void Release();
    }

    public interface IHttpClient : IDisposable
    {
        IHttpConnection Send(HttpRequest request, HttpResponseDelegate del = null);

        //Use to set game related data if the implementation needs it
        string Config { set; }

        event HttpRequestDelegate RequestSetup;
    }
}
