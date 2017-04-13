using SocialPoint.Base;
using SocialPoint.IO;

namespace SocialPoint.Network
{
    /**
     * a real clientId should never be 0
     */
    public interface INetworkServerDelegate
    {
        void OnServerStarted();

        void OnServerStopped();

        void OnClientConnected(byte clientId);

        void OnClientDisconnected(byte clientId);

        void OnMessageReceived(NetworkMessageData data);

        void OnNetworkError(Error err);
    }

    public interface INetworkServer : INetworkMessageSender
    {
        bool Running{ get; }

        string Id { get; }

        void Start();

        void Stop();

        void Fail(Error err);

        void AddDelegate(INetworkServerDelegate dlg);

        void RemoveDelegate(INetworkServerDelegate dlg);

        void RegisterReceiver(INetworkMessageReceiver receiver);

        int GetTimestamp();

        bool LatencySupported{ get; }
    }
}
