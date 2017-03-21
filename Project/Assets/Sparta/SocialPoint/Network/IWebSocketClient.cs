namespace SocialPoint.Network
{
    public interface IWebSocketClient : INetworkClient
    {
        string ConnectedUrl { get; }

        bool Connecting { get; }

        string[] Urls { get; set; }

        string Proxy { get; set; }

        void Ping();

        void OnWillGoBackground();

        void OnWasOnBackground();
    }
}
