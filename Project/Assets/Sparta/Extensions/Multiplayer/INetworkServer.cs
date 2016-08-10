using SocialPoint.Base;

namespace SocialPoint.Multiplayer
{
    public interface INetworkServerDelegate
    {
        void OnStarted();
        void OnStopped();
        void OnClientConnected(byte clientId);
        void OnClientDisconnected(byte clientId);
        void OnMessageReceived(byte clientId, ReceivedNetworkMessage msg);
        void OnError(Error err);
    }

    public interface INetworkServer
    {
        void Start();
        void Stop();
        INetworkMessage CreateMessage(NetworkMessageInfo info);
        void AddDelegate(INetworkServerDelegate dlg);
        void RemoveDelegate(INetworkServerDelegate dlg);
    }
}
