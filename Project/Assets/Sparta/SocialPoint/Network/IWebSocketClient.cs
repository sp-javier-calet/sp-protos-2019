namespace SocialPoint.Network
{
    public interface IWebSocketClient : INetworkClient
    {
        string ConnectedUrl { get; }

        bool Connecting { get; }

        bool InStandby { get; }

        string[] Urls { get; set; }

        string Proxy { get; set; }

        void Ping();

        void OnWillGoBackground();

        void OnWasOnBackground();
    }
}
