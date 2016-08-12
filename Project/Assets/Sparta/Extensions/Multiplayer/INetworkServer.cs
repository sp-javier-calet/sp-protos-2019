using SocialPoint.Base;

namespace SocialPoint.Multiplayer
{
    /**
     * a real clientId should never be 0
     */
    public interface INetworkServerDelegate
    {
        void OnStarted();
        void OnStopped();
        void OnClientConnected(byte clientId);
        void OnClientDisconnected(byte clientId);
        void OnMessageReceived(NetworkMessageData data);
        void OnError(Error err);
    }

    public interface INetworkServer
    {
        bool Running{ get; }
        void Start();
        void Stop();
        INetworkMessage CreateMessage(NetworkMessageData info);
        void AddDelegate(INetworkServerDelegate dlg);
        void RemoveDelegate(INetworkServerDelegate dlg);
        void RegisterReceiver(INetworkMessageReceiver receiver);
    }
}
