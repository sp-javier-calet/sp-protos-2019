namespace SocialPoint.Network
{
    public interface IWebSocketClient : INetworkClient
    {
        string ConnectedUrl { get; }

        string[] Urls { get; set; }

        string Proxy { get; set; }

        void Ping();
    }
}
