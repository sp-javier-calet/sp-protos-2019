using SocialPoint.Base;

namespace SocialPoint.Network
{
    public interface INetworkClientDelegate
    {
        void OnClientConnected();

        void OnClientDisconnected();

        void OnMessageReceived(NetworkMessageData data);

        void OnNetworkError(Error err);
    }

    public interface INetworkClient : INetworkMessageSender
    {
        byte ClientId{ get; }

        bool Connected{ get; }

        void Connect();

        void Disconnect();

        void AddDelegate(INetworkClientDelegate dlg);

        void RemoveDelegate(INetworkClientDelegate dlg);

        void RegisterReceiver(INetworkMessageReceiver receiver);

        /**
         * should return client delay in milliseconds
         * serverTimestamp should be sent in a message
         * from the server to the clients
         */
        int GetDelay(int networkTimestamp);

        bool LatencySupported{ get; }

        int Latency{ get; }
    }
}