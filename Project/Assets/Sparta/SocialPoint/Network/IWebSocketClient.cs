namespace SocialPoint.Network
{
    public interface IWebSocketClient : INetworkClient
    {
        string Url { get; set; }

        void Ping();
    }
}