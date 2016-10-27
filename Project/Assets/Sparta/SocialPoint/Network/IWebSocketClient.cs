namespace SocialPoint.Network
{
    public interface IWebSocketClient : INetworkClient
    {
        string Url { get; set; }

        string Proxy { get; set; }

        void Ping();
    }
}
