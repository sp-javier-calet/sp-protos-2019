using System;

namespace SocialPoint.Network
{
    public delegate void HttpStreamRequestDelegate(HttpRequest r);
    public delegate void HttpStreamClosedDelegate(HttpResponse r);

    public interface IHttpStream : IHttpConnection
    {
        bool Active { get; }

        event Action<byte[]> DataReceived;

        void SendData(byte[] data);
    }

    public interface IHttpStreamClient : IDisposable
    {
        IHttpStream Connect(HttpRequest request, HttpStreamClosedDelegate del = null);

        //Use to set game related data if the implementation needs it
        string Config { set; }

        event HttpStreamRequestDelegate RequestSetup;
    }
}