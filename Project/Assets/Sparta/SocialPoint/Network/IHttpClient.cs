using System;

namespace SocialPoint.Network
{
    public delegate void HttpRequestDelegate(HttpRequest r);
    public delegate void HttpResponseDelegate(HttpResponse r);

    public interface IHttpStream
    {
        event Action<byte[]> DataReceived;

        void SendData(byte[] data);
    }

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

        /// <summary>
        /// Gets the http stream, if available
        /// </summary>
        /// <value>The http stream. Null if the connection is not streamed.</value>
        IHttpStream Stream { get; }
    }

    public interface IHttpClient : IDisposable
    {
        IHttpConnection Send(HttpRequest request, HttpResponseDelegate del = null);

        [Obsolete("Please use the RequestSetup event to set default request values")]
        string DefaultProxy { set; }

        //Use to set game related data if the implementation needs it
        string Config { set; }

        event HttpRequestDelegate RequestSetup;

    }
}
